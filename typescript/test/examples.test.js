// Runs both examples against moca-spec's support-kb example package.
// Needs a moca-spec checkout beside this repository, or MOCA_SPEC_DIR.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = fileURLToPath(new URL('..', import.meta.url));
const spec = process.env.MOCA_SPEC_DIR ?? join(here, '..', '..', 'moca-spec');
const kb = join(spec, 'examples', 'support-kb');
const run = (script, ...args) => spawnSync(process.execPath, [join(here, script), ...args], { encoding: 'utf8' });

test('open prints the digest and capabilities', () => {
  const r = run('cli.js', 'open', kb);
  assert.equal(r.status, 0, r.stderr);
  assert.match(r.stdout, /digest\s+sha256:[a-f0-9]{64}/);
  assert.match(r.stdout, /capabilities\s+core/);
});

test('search returns cited results', () => {
  const r = run('cli.js', 'search', kb, 'refund', 'window');
  assert.equal(r.status, 0, r.stderr);
  assert.match(r.stdout, /^\[1\] .+\(https:\/\/.+#.+\)/m);
});

test('chat --show-prompt hands the sources to the model as data, not instructions', () => {
  const r = run('chat.js', kb, 'How', 'long', 'is', 'the', 'refund', 'window?', '--show-prompt');
  assert.equal(r.status, 0, r.stderr);
  const [system, user] = JSON.parse(r.stdout);
  assert.equal(system.role, 'system');
  assert.doesNotMatch(system.content, /<source/);
  assert.match(user.content, /<source n="1" ref="https:\/\/.+">/);
});
