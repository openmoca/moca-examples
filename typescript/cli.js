#!/usr/bin/env node
// Open a MOCA package, search it and cite what you find, with the reference Reader.
//
//   node cli.js open   <package>
//   node cli.js search <package> <query...>
//   node cli.js get    <package> <node-ref>
//
// <package> is a folder with a moca.json, or a .moca archive.
import { readPackage, Library, Search, LexicalBackend } from '@openmoca/moca-core';

const [command, target, ...rest] = process.argv.slice(2);
if (!['open', 'search', 'get'].includes(command) || !target || (command !== 'open' && rest.length === 0)) {
  console.error('usage: node cli.js open <package> | search <package> <query...> | get <package> <node-ref>');
  process.exit(2);
}

// Reading checks the package: its digest, its files, its evidence. A broken
// package is not an exception; it comes back with valid: false and diagnostics.
const pkg = await readPackage(target);
if (!pkg.valid) {
  for (const d of pkg.diagnostics) console.error(`${d.severity} ${d.code} ${d.file ?? ''} ${d.message}`);
  process.exit(1);
}

const library = new Library().add(pkg);

if (command === 'open') {
  console.log(`${pkg.manifest.id} ${pkg.manifest.version}`);
  console.log(`digest        ${pkg.digest}`);
  console.log(`capabilities  ${pkg.capabilities.join(', ')}`);
  console.log(`nodes         ${library.citations().length}`);
  for (const d of pkg.diagnostics) console.log(`${d.severity.padEnd(13)} ${d.code} ${d.message}`);
}

if (command === 'search') {
  // One search entry point. The Reader applies the default retrieval policy
  // to every hit (nothing out of force, superseded or deprecated) before it
  // returns, whatever the backend found.
  const search = new Search(library, { backend: new LexicalBackend(library) });
  const records = await search.search(rest.join(' '), { limit: 5 });
  if (records.length === 0) console.log('No results.');
  records.forEach((r, i) => console.log(`[${i + 1}] ${r.node.title ?? r.node.path}  (${r.node.ref})  score ${r.score.toFixed(2)}`));
}

if (command === 'get') {
  // A citation record: the text, plus where it came from and what vouches for it.
  const record = library.get(rest[0]);
  if (!record) {
    console.error(`No node ${rest[0]} in this package.`);
    process.exit(1);
  }
  console.log(JSON.stringify(record, null, 2));
}
