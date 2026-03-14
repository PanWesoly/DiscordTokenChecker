# Changelog

All notable changes to **Discord Token Checker** are documented here.

---

## [1.2.0] — 2026-03-13

### Added
- **Proxy support** — load proxies from `.txt` file in `IP:PORT` or `IP:PORT:USER:PASS` format; rotates automatically across threads
- **Proxyless fallback** — if no proxies are loaded, checker runs without proxy (direct connection)
- **Result tabs** — filter results by `All`, `Valid`, `Invalid`, `Errors` via tab bar above the data grid
- **Discord Webhook notifications** — sends a rich embed (blurple color, all token details in fields) to a configured webhook URL on every valid hit
- **Telegram Bot notifications** — sends a formatted HTML message with emojis to a configured bot/chat on every valid hit
- **NOTIFICATIONS panel** in the left sidebar with fields for Discord Webhook URL, Telegram Bot Token, and Telegram Chat ID
- **Custom Slider style** — blurple gradient track, circular gradient thumb with glow effect and hover/drag scale animation
- **Custom ScrollBar style** — slim dark scrollbars that highlight blurple on drag

### Changed
- Left panel reorganized: Tokens → Proxies → Threads → Controls → Export → Notifications → Statistics
- Proxy status indicator shows live count and mode (`Proxyless` / `Rotating`)

---

## [1.1.0] — 2026-03-13

### Added
- Multi-threaded token checking engine (`SemaphoreSlim` concurrency, configurable thread count via slider)
- Live statistics panel: Valid / Invalid / Errors / CPM counters
- Progress bar with `checked / total` counter in status bar
- Export Hits (valid only) and Export All buttons — saves `.txt` files
- Discord-style dark theme (`#1E1F22` / `#2B2D31` / `#313338` palette)
- Custom button styles: Primary (blurple), Secondary (dark), Danger (red)
- DataGrid with alternating row colors, no gridlines, custom column headers

### Checks performed per token
1. `GET /api/v9/users/@me/affinities/users` — token validity
2. `GET /api/v9/users/@me` — basic user info (email, phone, locale, 2FA, verified)
3. `GET /api/v9/users/{id}/profile` — username, nitro details, connected accounts
4. `GET /api/v9/users/@me/billing/payment-sources` — payment info
5. `GET /api/v9/users/@me/applications/521842831262875670/entitlements` — nitro balance

---

## [1.0.0] — 2026-03-12

### Added
- Initial release based on OpenBullet2 config by **grey** (`DISCORD TOKEN CHECKER GREY v1.1.2`)
- Converted to standalone .NET 8 WPF `.exe` (self-contained, `win-x64`)
- Basic token checking against Discord API v9 with all required headers (`X-Super-Properties`, `User-Agent`, etc.)
