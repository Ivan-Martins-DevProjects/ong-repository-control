# Design Language — Storager

## Design direction

Organic, warm, trustworthy. Forest greens + terracota accents on a warm cream base. Conveys nature, growth, and reliability — appropriate for an NGO inventory system.

## Audited surfaces

- Global styles (`src/styles.css`)
- App shell (`src/app/app.html`, `app.css`)
- Sidebar (`src/app/components/sidebar/`)
- Login page (`src/app/modules/login/`)
- Dashboard (`src/app/modules/dashboard/`)

## Design sources

- Custom CSS with CSS custom properties (no framework)
- Fonts: Outfit (display/headings), Inter (body)
- Icons: Material Symbols Outlined

## Color palette

### Core

| Token | Value | Usage |
|-------|-------|-------|
| `--c-bg` | `#f5f3ee` | Page background (warm cream) |
| `--c-surface` | `#ffffff` | Cards, panels, sidebar |
| `--c-surface-raised` | `#fafaf7` | Hover states, elevated surfaces |
| `--c-border` | `#e4e1da` | Borders, dividers |
| `--c-border-subtle` | `#eceae5` | Subtle separators |

### Forest

| Token | Value | Usage |
|-------|-------|-------|
| `--c-forest` | `#1b3a2d` | Primary brand, sidebar logo mark, submit buttons |
| `--c-forest-mid` | `#2d5a45` | Hover primary, active nav |
| `--c-forest-light` | `#3d7358` | Secondary forest |
| `--c-forest-pale` | `#e8f0eb` | Active nav background, avatar background |

### Terracota

| Token | Value | Usage |
|-------|-------|-------|
| `--c-terra` | `#c4704b` | Accent, focus rings, brand icon, links |
| `--c-terra-dim` | `#a85d3c` | Hover accent |
| `--c-terra-pale` | `#faf0eb` | Stat icon background |

### Sage

| Token | Value | Usage |
|-------|-------|-------|
| `--c-sage` | `#7b9e8a` | Muted green |
| `--c-sage-light` | `#a3c4b0` | Light sage |
| `--c-sage-pale` | `#f0f5f2` | Badge background |

### Text

| Token | Value | Usage |
|-------|-------|-------|
| `--c-text` | `#1a2e23` | Primary text (dark forest) |
| `--c-text-secondary` | `#5c7a6b` | Secondary text, labels |
| `--c-text-muted` | `#8fa69a` | Tertiary text, placeholders |
| `--c-text-inverse` | `#f5f3ee` | Text on dark backgrounds |

### Semantic

| Token | Value | Usage |
|-------|-------|-------|
| `--c-success` | `#2d8b57` | Entry, positive |
| `--c-success-bg` | `#ecf7f0` | Entry background |
| `--c-warning` | `#d4943a` | Warning states |
| `--c-warning-bg` | `#fdf6eb` | Warning background |
| `--c-danger` | `#c44b4b` | Exit, destructive |
| `--c-danger-bg` | `#fdf0f0` | Error background |
| `--c-info` | `#4a90c4` | Informational |
| `--c-info-bg` | `#eef5fb` | Info background |
| `--c-purple` | `#8b6cc4` | Process badge |
| `--c-purple-bg` | `#f3effc` | Process badge background |

## Typography

- **Display:** Outfit — headings, buttons, stat values, brand name
- **Body:** Inter — labels, inputs, descriptions, nav items
- Headings use `text-wrap: balance`
- Body text uses `text-wrap: pretty`
- Data values use `font-variant-numeric: tabular-nums`

## Spacing & radius

| Token | Value | Usage |
|-------|-------|-------|
| `--r-xs` | `6px` | Small icons, avatars |
| `--r-sm` | `8px` | Buttons, inputs, links |
| `--r-md` | `12px` | Cards, panels |
| `--r-lg` | `16px` | Large containers |
| `--r-xl` | `20px` | Extra large |
| `--r-pill` | `9999px` | Badges, avatars |
| `--sidebar-w` | `260px` | Sidebar width |

## Shadows

| Token | Value | Usage |
|-------|-------|-------|
| `--shadow-xs` | `0 1px 2px rgba(26,46,35,0.04)` | Subtle cards |
| `--shadow-sm` | `0 1px 3px rgba(26,46,35,0.06)` | Cards, default elevation |
| `--shadow-md` | `0 4px 12px rgba(26,46,35,0.08)` | Hover lift |
| `--shadow-lg` | `0 8px 24px rgba(26,46,35,0.1)` | Mobile sidebar, modals |
| `--shadow-focus` | `0 0 0 3px rgba(196,112,75,0.2)` | Focus ring (terracota) |

## Layout

- **App shell:** Sidebar (260px fixed, white) + main content (warm cream bg)
- **Sidebar:** White background, forest logo mark, nav with active state in forest-pale, user avatar with initial, logout at bottom
- **Mobile:** Sidebar slides in from left with backdrop blur overlay, hamburger toggle
- **Page content:** Max-width 1280px, centered, 2rem 2.5rem padding
- **Dashboard:** Greeting header → 3 stat cards (grid) → 2-column charts → activity list

## Login page

- Split layout: 52% forest green brand panel / 48% cream form panel
- Brand panel: decorative watermark "S", radial gradient glows (terracota + sage)
- Form panel: clean card with inputs, submit button in forest green
- Mode toggle link in terracota

## Interaction patterns

- Nav links: hover subtle raise, active uses forest-pale bg + forest text + filled icon
- Cards: hover subtle border darkening
- Inputs: border transitions to terracota on focus with ring
- Buttons: forest bg, hover mid-forest, active scale
- Focus ring: 3px terracota glow on all interactive elements
- All transitions use `var(--ease)` cubic-bezier(0.22, 1, 0.36, 1)

## Accessibility

- All interactive elements have visible focus rings (terracota glow)
- Icon-only buttons have `aria-label`
- Decorative icons marked with `aria-hidden`
- Nav regions labeled with `aria-label`
- Stat cards use `role="list"` / `role="listitem"`
- Charts wrapped with `role="img"` and `aria-label`
- Loading states use `role="status"` + `aria-live="polite"`
- Section headings linked via `aria-labelledby`
- `prefers-reduced-motion`: disables all transitions and animations
- Sidebar trap: overlay closes on click, backdrop blur

## Animation

- All transitions use `var(--ease)` cubic-bezier(0.22, 1, 0.36, 1)
- Sidebar slide: `transform: translateX()` — compositor-only
- Hover states: `background`, `color`, `border-color` transitions (150ms)
- Focus ring: `box-shadow` transition (200ms)
- Global `prefers-reduced-motion` kills all transitions

## Governing owners

- **Global tokens:** `src/styles.css` — single source of truth
- **App shell:** `src/app/app.html`, `app.css`
- **Sidebar:** `src/app/components/sidebar/`
- **Login:** `src/app/modules/login/`
- **Dashboard:** `src/app/modules/dashboard/`
