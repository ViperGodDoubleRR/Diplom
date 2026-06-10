type SocialUserRaw = Record<string, unknown>;

export type SocialListUser = {
  id: string;
  login: string;
  tag?: string;
  avatarUrl: string | null;
  avatarIsVideo: boolean;
};

function readString(raw: SocialUserRaw, ...keys: string[]): string | null {
  for (const key of keys) {
    const value = raw[key];
    if (typeof value === "string" && value.trim()) {
      return value.trim();
    }
  }

  return null;
}

function readBoolean(raw: SocialUserRaw, ...keys: string[]): boolean {
  for (const key of keys) {
    const value = raw[key];
    if (typeof value === "boolean") {
      return value;
    }
  }

  return false;
}

export function normalizeSocialUser(raw: SocialUserRaw): SocialListUser {
  const idValue = raw.id ?? raw.Id;
  const id = idValue != null ? String(idValue) : "";
  const login = readString(raw, "login", "Login") ?? "User";
  const tag = readString(raw, "tag", "Tag") ?? undefined;

  return {
    id,
    login,
    tag,
    avatarUrl: readString(raw, "avatarUrl", "AvatarUrl"),
    avatarIsVideo: readBoolean(raw, "avatarIsVideo", "AvatarIsVideo"),
  };
}

export function normalizeSocialUsers(rawList: unknown): SocialListUser[] {
  if (!Array.isArray(rawList)) {
    return [];
  }

  return rawList.map((item) => normalizeSocialUser(item as SocialUserRaw));
}

export function normalizeUserSearchTerm(query: string): string {
  return query.trim().replace(/^@+/, "");
}

export function filterSocialUsersByQuery(
  users: SocialListUser[],
  query: string
): SocialListUser[] {
  const term = query.trim().toLowerCase();
  if (!term) return users;

  return users.filter((user) => {
    const login = user.login.toLowerCase();
    const tag = user.tag?.toLowerCase() ?? "";
    return login.includes(term) || tag.includes(term);
  });
}

export function excludeSocialUsersByIds(
  users: SocialListUser[],
  excludeIds: Iterable<string>
): SocialListUser[] {
  const excluded = new Set(
    Array.from(excludeIds, (id) => id.toLowerCase()).filter(Boolean)
  );

  if (!excluded.size) return users;

  return users.filter((user) => !excluded.has(user.id.toLowerCase()));
}
