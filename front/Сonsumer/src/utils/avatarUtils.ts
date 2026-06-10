const AVATAR_COLORS = [
  "#4163FC",
  "#5B8DEF",
  "#7C5CFC",
  "#2ECC71",
  "#1ABC9C",
  "#E67E22",
  "#E74C3C",
  "#9B59B6",
  "#3498DB",
  "#16A085",
];

export function getAvatarInitials(name?: string | null): string {
  const value = (name ?? "").trim();
  if (!value) return "?";

  const letters = [...value].filter((char) => /\p{L}|\p{N}/u.test(char));
  const source = letters.length ? letters.join("") : value;

  return source.slice(0, 2).toUpperCase();
}

export function getAvatarColor(name?: string | null): string {
  const value = (name ?? "").trim() || "?";

  let hash = 0;
  for (let i = 0; i < value.length; i++) {
    hash = value.charCodeAt(i) + ((hash << 5) - hash);
  }

  const index = Math.abs(hash) % AVATAR_COLORS.length;
  return AVATAR_COLORS[index] ?? "#4163FC";
}
