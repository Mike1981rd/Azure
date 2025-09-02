#!/usr/bin/env node
const { spawn } = require('child_process');
const fs = require('fs');

function frame(obj){ const b = JSON.stringify(obj); return `Content-Length: ${Buffer.byteLength(b)}\r\n\r\n${b}`; }

class Reader{ constructor(stream,on){ this.buf=Buffer.alloc(0); stream.on('data',c=>{ this.buf=Buffer.concat([this.buf,c]); for(;;){ const i=this.buf.indexOf('\r\n\r\n'); if(i===-1) return; const m=/Content-Length:\s*(\d+)/i.exec(this.buf.slice(0,i).toString()); if(!m){ this.buf=this.buf.slice(i+4); continue; } const len=+m[1]; const tot=i+4+len; if(this.buf.length<tot) return; const body=this.buf.slice(i+4,tot).toString(); this.buf=this.buf.slice(tot); try{ on(JSON.parse(body)); }catch{} } }); } }

function readMcpEnv(){ const j=JSON.parse(fs.readFileSync('.mcp.json','utf8')); const env=j.mcpServers?.supabase?.env||{}; if(!env.SUPABASE_URL||!env.SUPABASE_ACCESS_TOKEN) throw new Error('Missing SUPABASE_URL or SUPABASE_ACCESS_TOKEN in .mcp.json'); return env; }

async function run(){ const env=readMcpEnv(); const child=spawn('mcp-server-supabase', [], { env:{...process.env, ...env}, stdio:['pipe','pipe','inherit'] });
  const pending=new Map(); let nextId=1; const send=(method,params)=>{ const id=nextId++; child.stdin.write(frame({jsonrpc:'2.0',id,method,params})); return new Promise((res,rej)=>{ const t=setTimeout(()=>{ pending.delete(id); rej(new Error('timeout:'+method)); }, 180000); pending.set(id,{res:(v)=>{clearTimeout(t);res(v)}, rej}); }); };
  new Reader(child.stdout,(msg)=>{ if(msg.id && pending.has(msg.id)){ const p=pending.get(msg.id); pending.delete(msg.id); if('result' in msg){ p.res(msg.result);} else { p.rej(new Error(msg.error?.message||'error')); } } else {
      if(msg.method==='notifications/message' || msg.method==='server/ready'){
        // ignore
      } else {
        try{ console.error('note:', JSON.stringify(msg)); }catch{}
      }
    } });
  const protoList=['2024-11-05','2024-10-07','2024-08-16'];
  let init=null, proto=null;
  for(const p of protoList){ try{ init=await send('initialize',{protocolVersion:p, capabilities:{}}); proto=p; break; } catch(e) { /* try next */ } }
  if(!init) throw new Error('initialize failed');
  const tools=await send('tools/list',{});
  const tool=(tools.tools||[]).find(t=>/execute_sql/i.test(t.name) || /sql/i.test(t.description||''));
  if(!tool) throw new Error('execute_sql tool not found');
  const sql = fs.readFileSync('/tmp/create_whatsapp.sql','utf8');
  const call=await send('tools/call',{ name: tool.name, arguments: { sql } });
  console.log('execute_sql.ok');
  try{ console.log((call && JSON.stringify(call).slice(0,600))||''); }catch{}
  child.stdin.end();
}
run().catch(e=>{ console.error('ERR', e.message); process.exit(1); });
