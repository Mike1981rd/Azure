#!/usr/bin/env node
const { spawn } = require('child_process');
const fs = require('fs');

function frame(obj){ const s=JSON.stringify(obj); return `Content-Length: ${Buffer.byteLength(s)}\r\n\r\n${s}`; }

const env = (()=>{ const j=JSON.parse(fs.readFileSync('.mcp.json','utf8')); return j.mcpServers.supabase.env; })();
const child=spawn('mcp-server-supabase', [], { env:{...process.env, ...env}, stdio:['pipe','pipe','inherit'] });

let buf=Buffer.alloc(0);
child.stdout.on('data', chunk=>{ buf=Buffer.concat([buf,chunk]); for(;;){ const i=buf.indexOf('\r\n\r\n'); if(i===-1) return; const m=/Content-Length:\s*(\d+)/i.exec(buf.slice(0,i).toString()); if(!m){ buf=buf.slice(i+4); continue;} const len=+m[1]; const tot=i+4+len; if(buf.length<tot) return; const body=buf.slice(i+4,tot).toString(); buf=buf.slice(tot); try{ const msg=JSON.parse(body); console.log('<<', JSON.stringify(msg)); }catch(e){ console.log('<<RAW', body); }
}});

function send(o){ process.stdout.write('>> '+JSON.stringify(o)+'\n'); child.stdin.write(frame(o)); }

setTimeout(()=>{
  send({ jsonrpc:'2.0', id:1, method:'initialize', params:{ protocolVersion:'2024-11-05' }});
}, 200);

setTimeout(()=>{ send({ jsonrpc:'2.0', id:2, method:'tools/list', params:{} }); }, 1500);

setTimeout(()=>{ console.error('DONE_WAIT'); process.exit(0); }, 15000);
