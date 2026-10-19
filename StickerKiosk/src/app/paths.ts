export const PATHS = {
  home: '/',
  stickers: '/sticker',
  studio: '/thiet-ke',
  checkout: '/thanh-toan',
  orders: '/don-hang',
  order: (code: string) => `/don-hang/${encodeURIComponent(code)}`,
  admin: '/quan-tri',
  adminLogin: '/quan-tri/dang-nhap',
  adminStickers: '/quan-tri/sticker',
  adminCases: '/quan-tri/op-lung',
  adminPhones: '/quan-tri/mau-may',
} as const;
