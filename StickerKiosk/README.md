# StickerKiosk

Website for the FORM Objects sticker counter. A customer picks a phone model, a case, and one printed sticker, then places the sticker on the case back in 3D. The robot applies that exact sticker at the stored center. Sticker size is physical and cannot change.

## Run

Start SQL Server and the API first (see the root of `SWD_Proj`), then:

```bash
npm install
npm run dev
```

Vite listens on 5173 and proxies `/api` and `/uploads` to `http://localhost:5174`.

`npm test` runs the placement math and session rules. `npm run build` typechecks and bundles.

## Pages

| Path | Page |
| --- | --- |
| `/` | Home, with a turning case and a sample sticker |
| `/sticker` | Sticker library: search, sort, size in mm, price |
| `/thiet-ke` | Design: model, case, sticker tray, 3D placement, total |
| `/thanh-toan` | Checkout: name, phone, payment method |
| `/don-hang/:code` | Order status and placement, also the receipt |
| `/quan-tri` | Admin (Manager or SuperAdmin): orders, stickers, cases, phone models |

## How placement works

Each case GLB is imported in `/quan-tri/op-lung`. The admin rotates it in quarter turns until the back faces the camera with the camera bump on top, and types the real height in mm. "Tính vùng dán" renders the back once on the GPU and marks every 1 mm cell that is flat, facing out, and not over a hole, step, or edge. That mask is saved with the case.

A placement is the sticker center in tenths of a millimetre from the top-left corner of the case back, seen looking at the back. The browser (`src/kiosk/sheet.ts`) and the API (`SWD-BE/Utilities/BackSheetMath.cs`) run the same integer test: every cell under the sticker must be printable. The 3D stage raycasts only the case, never the phone, and draws the sticker with `DecalGeometry` projected straight along the back axis at its real size.

Code layout:

- `src/kiosk`: session state, catalog types, placement math. No Three.js, no HTTP.
- `src/api`: HTTP calls and parsing. The order is five requests that can resume after a failure.
- `src/stage`: Three.js. `canonical.ts` is the only place a GLB is rotated or scaled.
- `src/pages`: public and admin pages.
