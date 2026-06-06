---
name: Khidmati Unified System
colors:
  surface: '#f5faf8'
  surface-dim: '#d6dbd9'
  surface-bright: '#f5faf8'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f0f5f2'
  surface-container: '#eaefed'
  surface-container-high: '#e4e9e7'
  surface-container-highest: '#dee4e1'
  on-surface: '#171d1c'
  on-surface-variant: '#3d4947'
  inverse-surface: '#2c3130'
  inverse-on-surface: '#edf2f0'
  outline: '#6d7a77'
  outline-variant: '#bcc9c6'
  surface-tint: '#006a61'
  primary: '#00685f'
  on-primary: '#ffffff'
  primary-container: '#008378'
  on-primary-container: '#f4fffc'
  inverse-primary: '#6bd8cb'
  secondary: '#3755c3'
  on-secondary: '#ffffff'
  secondary-container: '#708cfd'
  on-secondary-container: '#00217a'
  tertiary: '#545c72'
  on-tertiary: '#ffffff'
  tertiary-container: '#6c748b'
  on-tertiary-container: '#fefcff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#89f5e7'
  primary-fixed-dim: '#6bd8cb'
  on-primary-fixed: '#00201d'
  on-primary-fixed-variant: '#005049'
  secondary-fixed: '#dde1ff'
  secondary-fixed-dim: '#b8c4ff'
  on-secondary-fixed: '#001453'
  on-secondary-fixed-variant: '#173bab'
  tertiary-fixed: '#dae2fd'
  tertiary-fixed-dim: '#bec6e0'
  on-tertiary-fixed: '#131b2e'
  on-tertiary-fixed-variant: '#3f465c'
  background: '#f5faf8'
  on-background: '#171d1c'
  surface-variant: '#dee4e1'
typography:
  display-lg:
    fontFamily: Inter
    fontSize: 48px
    fontWeight: '700'
    lineHeight: '1.2'
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '700'
    lineHeight: '1.25'
    letterSpacing: -0.01em
  headline-lg-mobile:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '700'
    lineHeight: '1.3'
  headline-md:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: '1.3'
  headline-sm:
    fontFamily: Inter
    fontSize: 20px
    fontWeight: '600'
    lineHeight: '1.4'
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.5'
  body-sm:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1.5'
  label-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '600'
    lineHeight: '1.2'
    letterSpacing: 0.01em
  label-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: '1.2'
    letterSpacing: 0.02em
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base: 8px
  xs: 4px
  sm: 12px
  md: 24px
  lg: 40px
  xl: 64px
  gutter: 24px
  margin-mobile: 16px
  margin-desktop: 48px
  max-width: 1280px
---

## Brand & Style
The design system is engineered for a high-trust service marketplace, balancing the efficiency of a utility tool with the approachability of a community-driven platform. The brand personality is professional, dependable, and transparent, aimed at both homeowners seeking help and skilled professionals managing their livelihood.

The design style follows a **Modern Corporate** aesthetic with a **Tactile** twist. It utilizes generous whitespace, a structured grid, and soft geometric shapes to reduce the cognitive load of navigating complex service categories and booking workflows. The UI should evoke a sense of calm and order, ensuring users feel secure during financial transactions and service scheduling.

## Colors
The palette is anchored by **Calm Teal**, symbolizing growth and freshness, used for primary actions and brand presence. **Trust Blue** is reserved for secondary navigation elements and links to reinforce stability. 

The background is intentionally kept to a very light gray (#F9FAFB) to allow white surface containers to "pop" via subtle shadows. Functional colors (Success, Warning, Error) follow industry standards to ensure immediate recognition of booking statuses. Neutrals are strictly Slate-based to maintain a cool, professional temperature across all text and borders.

## Typography
The design system exclusively uses **Inter** for its exceptional legibility and systematic feel. The hierarchy is driven by weight and scale rather than font mixing. 

- **Headlines:** Use Bold (700) or SemiBold (600) with slight negative letter-spacing for a modern, compact look.
- **Body Text:** Use Regular (400) weight for maximum readability in service descriptions and reviews.
- **Labels:** Use SemiBold (600) for button text and status badges to ensure they stand out against background colors.
- **Mobile scaling:** Headlines scale down on mobile to prevent awkward line breaks in service titles.

## Layout & Spacing
This design system employs a **8px linear scale** for consistent rhythm. 

- **Grid:** A 12-column fluid grid is used for desktop (max-width 1280px). On mobile, the layout collapses to a single column with 16px side margins.
- **Service Lists:** Category grids should use 24px gutters to allow each card enough "breathable" space.
- **Sidebars:** Provider dashboards utilize a fixed-width sidebar (280px) that collapses into a bottom navigation bar on mobile devices.
- **Vertical Spacing:** Use `lg` (40px) spacing between major sections and `md` (24px) between elements within a section (e.g., a card title and its content).

## Elevation & Depth
Depth is created using a **Tonal Layering** approach combined with **Ambient Shadows**. 

1. **Surface Low:** The main background (#F9FAFB) is the lowest level.
2. **Surface High:** White cards and containers (#FFFFFF). These use a soft, wide-dispersion shadow: `0px 4px 20px rgba(15, 23, 42, 0.05)`.
3. **Elevated:** Active elements or modals. These use a more pronounced shadow: `0px 10px 30px rgba(15, 23, 42, 0.1)`.

Avoid harsh borders. Instead, use 1px subtle strokes in Slate-200 (#E2E8F0) to define boundaries on white surfaces where shadows are not appropriate.

## Shapes
The shape language is defined as **Rounded**, promoting a friendly and approachable user experience. 

- **Standard Elements:** Buttons, input fields, and small cards use a 0.5rem (8px) radius.
- **Service Cards:** Use `rounded-lg` (16px) to create a distinct, modern "object" feel.
- **Status Badges & Chips:** Use a full pill-shape (999px) to distinguish them from interactive buttons.
- **Avatars:** Always circular to represent the human element of the marketplace.

## Components

### Buttons
- **Primary:** Calm Teal background with White text. High-contrast, 8px radius.
- **Secondary:** White background with Calm Teal border and text. 
- **Sizes:** Large (56px) for main CTAs like "Book Now"; Medium (44px) for standard actions.

### Service Category Cards
- White background, 16px radius, subtle shadow.
- Top-aligned icon in a soft teal circle (10% opacity).
- Centered or left-aligned text using `headline-sm`.

### Status Badges
- **Completed:** Soft Green background (10% opacity) with dark green text.
- **Disputed:** Soft Red background (10% opacity) with dark red text.
- All badges are pill-shaped with `label-sm` bold text.

### Inputs & Forms
- 8px radius with a Slate-200 border. 
- On focus, the border changes to Primary Teal with a subtle 2px teal glow (20% opacity).
- Labels must be `label-md` and placed above the input field.

### Wallet & Earnings Cards
- Use the Secondary Trust Blue as a solid background or a subtle gradient.
- Currency should be displayed in `headline-lg` for clear visibility.
- Include a "Withdraw" primary button within the card for immediate action.

### Tables (Admin)
- Minimalist style: no vertical lines, only horizontal Slate-100 dividers.
- Row hover state: Light Teal background (#F0FDFA).