import { mkdir, copyFile } from 'node:fs/promises';
import { dirname, resolve } from 'node:path';

const root = resolve(process.cwd());

const copies = [
  {
    from: 'node_modules/htmx.org/dist/htmx.min.js',
    to: 'wwwroot/lib/htmx/htmx.min.js',
  },
];

let copiedCount = 0;

for (const { from, to } of copies) {
  const src = resolve(root, from);
  const dst = resolve(root, to);

  await mkdir(dirname(dst), { recursive: true });
  await copyFile(src, dst);
  copiedCount += 1;
  process.stdout.write(`Copied ${from} -> ${to}\n`);
}

process.stdout.write(`Done. Copied ${copiedCount} file(s).\n`);
