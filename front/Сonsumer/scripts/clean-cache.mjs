import { existsSync, rmSync } from "node:fs";
import { join } from "node:path";
import { fileURLToPath } from "node:url";

const root = fileURLToPath(new URL("..", import.meta.url));

const targets = [
  join(root, "node_modules", ".vite"),
  join(root, "node_modules", ".cache"),
  join(root, "dist"),
  join(root, "dist-ssr"),
  join(root, ".eslintcache"),
  join(root, "tsconfig.app.tsbuildinfo"),
  join(root, "tsconfig.node.tsbuildinfo"),
];

let removed = 0;

for (const target of targets) {
  if (!existsSync(target)) continue;

  rmSync(target, { recursive: true, force: true });
  removed += 1;
  console.log(`removed: ${target}`);
}

console.log(
  removed > 0
    ? `Cache cleanup done (${removed} item(s)).`
    : "Cache cleanup done (nothing to remove)."
);
