#!/usr/bin/env node
// Minimal MCP stdio client to validate mcp-server-supabase tools
const fs = require('fs');
const { spawn } = require('child_process');

function readMcpConfig(path) {
  try {
    const raw = fs.readFileSync(path, 'utf8');
    const json = JSON.parse(raw);
    const srv = json.mcpServers?.supabase;
    if (!srv) throw new Error('supabase server not found in .mcp.json');
    const env = srv.env || {};
    return { env };
  } catch (e) {
    throw new Error(`Failed to read ${path}: ${e.message}`);
  }
}

function frame(obj) {
  const body = JSON.stringify(obj);
  return `Content-Length: ${Buffer.byteLength(body, 'utf8')}` + "\r\n\r\n" + body;
}

class StdioReader {
  constructor(stream, onMessage) {
    this.stream = stream;
    this.buffer = Buffer.alloc(0);
    this.onMessage = onMessage;
    stream.on('data', (chunk) => this._onData(chunk));
  }
  _onData(chunk) {
    this.buffer = Buffer.concat([this.buffer, chunk]);
    while (true) {
      const headerEnd = this.buffer.indexOf('\r\n\r\n');
      if (headerEnd === -1) return; // need more
      const header = this.buffer.slice(0, headerEnd).toString('utf8');
      const match = header.match(/Content-Length:\s*(\d+)/i);
      if (!match) {
        // drop garbage
        this.buffer = this.buffer.slice(headerEnd + 4);
        continue;
      }
      const len = parseInt(match[1], 10);
      const total = headerEnd + 4 + len;
      if (this.buffer.length < total) return; // need more body
      const bodyBuf = this.buffer.slice(headerEnd + 4, total);
      this.buffer = this.buffer.slice(total);
      try {
        const msg = JSON.parse(bodyBuf.toString('utf8'));
        this.onMessage(msg);
      } catch (_) {
        // ignore parse errors
      }
    }
  }
}

async function main() {
  const { env } = readMcpConfig('.mcp.json');
  if (!env.SUPABASE_URL || !env.SUPABASE_ACCESS_TOKEN) {
    console.error('Missing SUPABASE_URL or SUPABASE_ACCESS_TOKEN in .mcp.json env');
    process.exit(1);
  }

  const child = spawn('mcp-server-supabase', [], {
    env: { ...process.env, SUPABASE_URL: env.SUPABASE_URL, SUPABASE_ACCESS_TOKEN: env.SUPABASE_ACCESS_TOKEN },
    stdio: ['pipe', 'pipe', 'inherit']
  });

  const pending = new Map();
  let nextId = 1;
  const send = (method, params) => {
    const id = nextId++;
    const req = { jsonrpc: '2.0', id, method, params };
    child.stdin.write(frame(req));
    return new Promise((resolve, reject) => {
      const timeout = setTimeout(() => {
        pending.delete(id);
        reject(new Error(`Timeout waiting response for ${method}`));
      }, 120000);
      pending.set(id, { resolve: (v) => { clearTimeout(timeout); resolve(v); }, reject });
    });
  };

  new StdioReader(child.stdout, (msg) => {
    if (msg.id && pending.has(msg.id)) {
      const { resolve, reject } = pending.get(msg.id);
      pending.delete(msg.id);
      if (Object.prototype.hasOwnProperty.call(msg, 'result')) resolve(msg.result);
      else reject(new Error(msg.error?.message || 'Unknown MCP error'));
    } else {
      // log server notifications/events to aid debugging
      try { console.log('notification:', JSON.stringify(msg).slice(0, 400)); } catch (_) {}
    }
  });

  try {
    const init = await send('initialize', {
      protocolVersion: '2024-11-05',
      capabilities: { tools: { list: true, call: true } },
      clientInfo: { name: 'wb-mcp-tester', version: '1.0.0' }
    });
    console.log('initialize.ok', !!init);
    const tools = await send('tools/list', {});
    console.log('tools:', tools?.tools?.map(t => t.name));
    // pick a list_tables-like tool
    const listTool = tools.tools.find(t => /list_tables/i.test(t.name)) || tools.tools[0];
    if (!listTool) throw new Error('No tools exposed');
    const callParams = { name: listTool.name, arguments: { schemas: ['public'] } };
    const res = await send('tools/call', callParams);
    console.log('call.result.summary:', (res?.content && JSON.stringify(res.content).slice(0, 400)) || res);
  } catch (e) {
    console.error('MCP test failed:', e.message);
    process.exit(2);
  } finally {
    child.stdin.end();
    setTimeout(() => child.kill('SIGKILL'), 500).unref();
  }
}

main();
