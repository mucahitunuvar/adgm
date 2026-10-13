# AGENTS.md (frontend)

> **Scope:** These instructions apply to everything under `frontend/` (the pnpm workspace that contains the four React applications and their shared packages).
>
> **Precedence:** For files under `frontend/`, this file is the more specific `AGENTS.md`. The root `AGENTS.md` still applies for everything that is not frontend-specific: security, data ownership, API contracts, the "stop before coding" rule, the architectural conflict rule, and the completion report. Where the two files disagree about the frontend, this file wins. Where a rule here conflicts with the backend contract or `SECURITY.md`, stop and report it.
>
> **Mandatory:** Read this file before analyzing, generating, moving, refactoring, or deleting frontend code.
>
> Do not treat existing violations as patterns to copy.
>
> **Core rule:** Do not bypass architecture to finish faster.

---

# 1. Mission

The frontend consists of four applications served from one host:

| App | Path | Audience | Technology |
|---|---|---|---|
| Public website | `/` | Everyone (anonymous by default) | Next.js (App Router), React 19, TypeScript strict |
| Candidate portal | `/portal/aday` | Logged-in candidates | React SPA (Vite) |
| Employer portal | `/portal/firma` | Logged-in employers | React SPA (Vite) |
| Admin portal | `/admin` | Logged-in staff | React SPA (Vite) |

The first application to be built is the **public website** (`apps/web`). The other three follow after the backend contracts they need are mature.

The frontend is a **client of the backend**. It never owns business rules, authorization, validation truth, or data. Product behavior lives in `PROJECT.md` and `DOMAIN.md`; decisions taken so far live in `claude/web-frontend-karar-ozeti.md` (until they are moved into a formal ADR).

---

# 2. Mandatory Reading

Before meaningful frontend work, read:

1. Root `AGENTS.md`
2. This file
3. `claude/web-frontend-devir-ozeti.md` and `claude/web-frontend-karar-ozeti.md`
4. Root `SECURITY.md` for anything touching authentication, sessions, tokens, forms, uploads, or personal data
5. Root `PERFORMANCE.md` for rendering, caching, and image decisions
6. Relevant ADRs under `docs/adr/` (for the Website module: ADR-024)
7. The OpenAPI document of the running backend for any endpoint you use

Do not repeatedly load unrelated documentation.

---

# 3. Process: Architecture First, Then Design, Then Code

The order for every new area is:

```text
Architecture / decisions
        ↓
Design (screens, states, content)
        ↓
Code
```

Do not jump to code (or to visual design) when decisions are still open. If a decision is missing, ask or list it as an open question. Do not invent it.

## Stop before coding

Before implementing a meaningful page, template, or feature, identify and state:

1. Which application and which route.
2. Which template or component it uses or introduces.
3. Which backend endpoint(s) and which OpenAPI operation(s) supply the data.
4. All UI states: loading, empty, error, partial, success, forbidden, not found.
5. Whether the page is public/cacheable or user-specific.
6. Authentication and authorization expectations (and what the backend enforces).
7. SEO needs: title, description, canonical, Open Graph, structured data.
8. Accessibility needs.
9. Analytics/consent implications (any third-party request).
10. Which files will be created, modified, moved, or deleted.

For broad changes, list those files before implementing.

## Architectural conflict rule

If a requirement conflicts with this file, the root `AGENTS.md`, `SECURITY.md`, the OpenAPI contract, or an ADR, **stop before coding** and explain:

```text
Conflict
Why it conflicts
Affected rule
Possible alternatives
Recommended solution
```

Do not silently invent a workaround.

---

# 4. Backend-First and API Contract Rule

The backend is the source of truth for domain rules, authorization, validation, and data.

* The OpenAPI document is the only source of truth for request/response shapes.
* Types and the API client are **generated** (`packages/api-client`). Never hand-write API types that the OpenAPI document already describes.
* Never guess an endpoint, field, or enum value. If the contract lacks something the UI needs, **do not fake it silently**. Record it as a backend gap in the "Backend gaps" list of the completion report and ask how to proceed.
* Mock data is allowed only in tests, Storybook-style previews, and clearly marked development fixtures. It must never ship in a production code path.
* Frontend validation is a convenience. The backend always validates again. Never rely on the client for security or business correctness.
* Never trust client state as proof of identity, role, ownership, or permission. Hiding a button or protecting a route is UX, not security.
* Do not change a backend contract to make UI work easier without raising it as a conflict.

---

# 5. Workspace Structure and Dependency Direction

```text
frontend/
├── apps/
│   ├── web/                # Public website (Next.js)
│   ├── candidate/          # Candidate portal (Vite SPA)
│   ├── employer/           # Employer portal (Vite SPA)
│   └── admin/              # Admin portal (Vite SPA)
└── packages/
    ├── api-client/         # Generated from OpenAPI + thin fetch wrapper
    └── design-tokens/      # Colors, radius, spacing, typography, themes
```

Rules:

* Apps may depend on packages. Packages never depend on apps.
* Apps never import from other apps.
* `packages/` stays intentionally small. Add a package only when two or more apps genuinely share it. Do not create a `shared`, `common`, `utils`, or `core` dumping ground.
* Shared UI components start inside the app that needs them. Promote to a package only after a second real consumer exists and the API is stable.
* The workspace uses `pnpm`. Do not add `package-lock.json` or `yarn.lock`.

## Inside an application: vertical slices

Organize by feature/use case, not by technical type.

```text
apps/web/src/
├── app/                    # Next.js routes (thin; compose features)
├── features/
│   ├── jobs/
│   │   ├── components/
│   │   ├── api/
│   │   └── ...
│   ├── events/
│   ├── content/
│   └── forms/
├── blocks/                 # Homepage/page block components + registry
├── components/             # Truly generic UI primitives only
├── lib/                    # Narrow, named modules (not a junk drawer)
└── i18n/                   # tr dictionary
```

Do not create global horizontal folders (`hooks/`, `types/`, `services/`) that hold every feature's code. Keep feature code next to its feature.

Route files in `app/` stay thin: fetch data, set metadata, compose feature components. No business logic.

---

# 6. One Component Per File and Naming

Mirrors the backend one-type-per-file rule.

* One React component per file. The file name equals the component name in PascalCase: `JobCard.tsx`, `FeaturedJobsSection.tsx`.
* One hook per file: `useThemePreference.ts`.
* One exported type/interface per file when it is a primary concept, named after the type. Small local prop types may live beside their single component.
* Test files sit beside the code: `JobCard.test.tsx`.
* Do not name files for convenience: no `utils.ts`, `helpers.ts`, `common.ts`, `types.ts`, `constants.ts`, `index.ts` bucket files holding unrelated things.
* Barrel (`index.ts`) files are allowed only as the public entry of a package. Do not use them inside apps to hide structure.
* Named exports by default. Default exports only where the framework requires them (Next.js `page.tsx`, `layout.tsx`).
* Generated files (the OpenAPI client output) are exempt from manual organization rules and must be marked generated.

---

# 7. Language Rules

* The user communicates in Turkish; assistant replies are in Turkish (see `CLAUDE.md`).
* Code, identifiers, comments, and commit messages are in English.
* All user-visible text is Turkish and lives in the `tr` dictionary under `i18n/`. **No hardcoded UI strings inside components.**
* Site is Turkish only for now. URLs have no language prefix. When more languages arrive, Turkish stays at the root and new languages live under `/en/...` so existing URLs never break.
* Format dates, numbers, and currency with `tr-TR` and the `Europe/Istanbul` time zone unless the backend says otherwise.
* Content (pages, news, events, jobs) comes from the API already translated. The dictionary is only for interface text.

---

# 8. Public Website: Rendering Strategy

The public website uses Next.js (App Router) for server rendering, because search engines and social previews need real HTML.

| Page type | Strategy |
|---|---|
| Home, content pages, news, announcements, stories, activities, projects | Server render + time-based revalidation (about 60 s unless the backend says otherwise) |
| Job list (`/ilanlar`, `?q=`, filters) | Server render, short revalidation |
| Job detail (`/ilanlar/{slug}`) | Cached; closed/removed job behavior follows the backend (404/410) |
| Event list and detail | Page shell cached; capacity and registration state fetched live in the browser |
| Forms, quick job search box, theme toggle, consent UI | Small client components |

Rules:

* Server Components by default. Add `"use client"` only for interactivity, and keep client components small and leaf-level.
* **Public pages must render the same HTML for every visitor.** Never read cookies, headers, or session inside a public page render. User-specific parts (header "Hesabım", event form prefill) resolve in the browser after load.
* `sitemap.xml` and `robots.txt` are generated by the backend. The reverse proxy routes them to the API. Do not generate them in Next.js.
* Structured data (schema.org JSON-LD) comes from the backend response. Do not recompute it in the frontend.
* Set `title`, `description`, canonical, and Open Graph metadata from the content's SEO fields.
* Maintenance mode follows the backend response (503 with `Retry-After`). Provide a proper maintenance page and a branded 404.
* Do not add on-demand cache invalidation (webhooks) unless it is a decided requirement.

---

# 9. Reserved Paths (Single Host)

All applications share one host. The reverse proxy routes by path:

```text
/api/**              → backend
/sitemap.xml         → backend
/robots.txt          → backend
/admin/**            → admin portal
/portal/aday/**      → candidate portal
/portal/firma/**     → employer portal
/**                  → public website (Next.js)
```

* These prefixes are reserved. Content slugs must never collide with them (the backend reserved-slug list must include them).
* Public site pages must not define routes under reserved prefixes.
* Portals are built with the correct base path (`/admin`, `/portal/aday`, `/portal/firma`).

---

# 10. Block System (Homepage and Pages)

The Website module composes pages from **blocks** that administrators arrange. The frontend must support this.

* Each block type maps to exactly one component through a **block registry** (`blocks/`): `type → component`.
* A block component receives typed props from the API payload. It does not fetch its own data unless the contract defines that block as data-bound.
* **Unknown block types are skipped safely** (no crash, no empty placeholder). Log them in development.
* **Empty blocks are hidden**: a jobs, events, news, or stories block with no items renders nothing.
* Adding a new block means: one component file, one registry entry, one test, and (if needed) a backend gap entry. It must not require touching other blocks.
* Homepage sections decided so far: slider, center introduction, numbers strip, featured jobs (newest N), how it works, video introduction, events, success stories, employer band, news, e-newsletter. The mapping of these sections to the ADR-024 block catalog is still pending. Do not guess it; raise it.
* The homepage pop-up appears on the homepage only.

---

# 11. API Client

* `packages/api-client` is generated from the OpenAPI document with `openapi-typescript` plus a thin fetch wrapper. Regenerate; do not edit generated output by hand.
* One wrapper handles base URL, headers, `AbortSignal`, retries (where safe), and error mapping.
* Errors arrive as `ProblemDetails`. Map them to typed UI errors. Never show raw server messages, stack traces, or technical details to users.
* Pass `AbortSignal` so navigating away cancels requests.
* Do not read environment secrets in browser code. Anything prefixed `NEXT_PUBLIC_` or `VITE_` is public by definition.
* List endpoints are paginated. Never request or render unbounded lists.

---

# 12. Authentication and Sessions

* The public website is anonymous. It has **no password fields and no credential handling**. `/giris` and `/uye-ol` are choice pages: Aday (left) and Firma (right) cards that send the visitor to the relevant portal's own sign-in/sign-up page (stacked on mobile).
* Credentials are entered only inside the portals.
* The only places the public site needs to know about a session: the header ("Hesabım" instead of "Giriş Yap") and event-form prefill for a logged-in candidate. Resolve both in the browser after load so pages stay cacheable.
* **Token storage is not decided yet.** Preferred direction: HttpOnly + Secure + SameSite cookies. Do not implement token storage until it is verified against the Identity module and `SECURITY.md`. Never store tokens in `localStorage` or `sessionStorage`. Never log tokens or put them in URLs.
* Client-side route guards in portals are UX only. The backend always verifies authentication and authorization.

---

# 13. Security

* Render HTML only through one `SafeHtml` component, and only from fields the backend has already sanitized. `dangerouslySetInnerHTML` is forbidden everywhere else.
* Content Security Policy is restrictive. Allowed exceptions are only what the product needs (Cloudflare Turnstile, `youtube-nocookie.com` for the video popup, Google Analytics after consent). Any new third-party origin requires an explicit decision.
* External links use `rel="noopener noreferrer"`.
* Forms: backend form engine defines fields. Bot protection uses Cloudflare Turnstile. Never log form contents.
* Never log personal data, tokens, or full CV content in the browser console or in any logging service.
* Uploads are validated by the backend. Client checks (type, size) are convenience only.
* Do not expose gender preference in any public job view. Military-service preference is shown on job detail (per backend contract). Follow the contract; do not add fields.

---

# 14. KVKK, Consent, and Analytics

* **Google Analytics 4** is the chosen analytics tool and is **opt-in**. No GA script, request, or cookie before the visitor accepts the "analytics" category.
* The consent banner has at least two categories: essential and analytics. A "Çerez tercihleri" link in the footer lets visitors change their choice at any time. The choice is stored in the browser.
* Analytics lives in **one component** driven by consent state. Do not scatter tracking calls. This keeps a later switch to a self-hosted tool a one-component change.
* YouTube is **click-to-load**: show a poster image and play button; load the `youtube-nocookie.com` iframe only after the click, inside the popup.
* Fonts are **self-hosted**. Do not load Google Fonts or other third-party font/CDN assets.
* Turnstile loads only on pages with forms and is disclosed in the KVKK notice.
* Do not add any other third-party script (chat widgets, pixels, embeds) without an explicit decision.
* The KVKK notice wording is owned by the project and its legal advisor. Do not invent legal text.

---

# 15. Design System

Design direction: **"Modern güven"** (modern trust). Rounded, glass accents, light and dark themes, sharp editorial typography, generous whitespace. It must not look like a generic template or "AI site".

## Tokens

All design values come from `packages/design-tokens` as CSS variables. **Never hardcode hex colors, radii, or shadows inside components.**

Palette:

```text
#071f32  #041725  #102a2e   deep navy / ink surfaces
#c44325  #92301b  #d94a2b   accent (red-orange family)
#0a686b                     turquoise (secondary)
#e6efed  #ffffff            light surfaces
```

Usage:

* Primary accent for buttons and calls to action: `#c44325`. Turquoise appears sparingly in icons, badges, and small decorative touches.
* `#d94a2b` (the bright tone) is for large text and decoration only. White small text on it fails contrast.
* Both themes must meet WCAG AA contrast. Verify each token pair, do not assume.
* Theme follows the system preference by default; a toggle lets the visitor override; the choice persists in the browser. Avoid a flash of the wrong theme on load.

## Shape and glass

* One proportional radius scale from tokens. Do not invent per-component radii.
* **Glass is allowed in only three places:** the sticky header, cards that sit on top of imagery (hero, video, success story), and the pop-up. All other content surfaces are matte.
* Glass over imagery must have a dark scrim underneath so text stays readable. Provide a non-blur fallback (solid translucent background) for browsers or modes without `backdrop-filter`, and keep blur light on mobile.

## Imagery and icons

* Starting content uses **stock photography**. Build image slots so real photos replace stock later without layout changes.
* Photography-led design. Icons are used only where an icon is actually needed; keep them simple and few (Lucide).
* Images come as backend-generated fixed-size variants. **Next.js image optimization is disabled**; use the backend variants through `srcset`/`sizes`. Never distort aspect ratio. Always provide `alt` text (decorative images use empty `alt`).
* Slider: desktop image required, mobile image optional (falls back to the desktop image).

## Motion

* Motion is restrained and purposeful. Respect `prefers-reduced-motion` everywhere.
* Carousels (Embla) pause on interaction and expose accessible controls. No autoplay that cannot be stopped.

---

# 16. Dependencies

Approved for the public website:

| Purpose | Choice |
|---|---|
| Framework | Next.js (App Router), React 19, TypeScript (`strict`) |
| Styling | Tailwind CSS driven by design tokens |
| Carousel | Embla Carousel |
| Animation | Motion (used sparingly) |
| API types/client | `openapi-typescript` + thin fetch wrapper |
| Icons | Lucide |
| Package manager | pnpm |

Rules:

* Do not add a package for a small inconvenience. Before adding one, check: does the framework or an approved package already do it, is it maintained, is the license compatible, what is its size cost, does it add operational complexity?
* No general UI component kit by default. If accessible headless primitives (dialog, menu, popover) are needed, **propose them before adding**.
* No AutoMapper-style magic, no global state library by default. Prefer server data, URL state, and local state. Propose a state library only when a concrete need appears.
* Third-party design/AI plugins and MCP servers are **not required** for this project and the codebase must not depend on them. Only npm packages above are part of the build.
* Pin the Node.js LTS version in `.nvmrc` and `engines`. Commit the lockfile.
* Do not upgrade critical dependencies blindly; check license and breaking changes first.

---

# 17. Accessibility

Target **WCAG 2.2 AA**.

* Semantic landmarks (`header`, `nav`, `main`, `footer`), correct heading order, one `h1` per page.
* Full keyboard operation. Visible focus. Skip-to-content link.
* Menus (desktop dropdowns, mobile drawer) and the video popup manage focus, trap it while open, close on `Escape`, and restore focus.
* Carousels: pause/play control, previous/next buttons, readable labels, no motion trap.
* Forms: labels tied to inputs, errors announced and linked, never color alone.
* Do not rely on color or hover alone to convey state.
* The consent banner and pop-up must not block keyboard or screen-reader users.

---

# 18. Performance

Principle: correctness, security, architecture, then performance. Measure before optimizing.

Targets for the public website (field/lab, mobile):

```text
LCP  < 2.5 s
INP  < 200 ms
CLS  < 0.1
```

* Prefer server rendering and minimal client JS. Watch the JS shipped per page.
* The hero/first slide image is prioritized; everything else is lazy-loaded with reserved dimensions to prevent layout shift.
* Load carousels, video popup, and animation code lazily where possible.
* Never fetch unbounded lists. Paginate.
* Do not add caching layers, extra libraries, or "optimizations" without a measurement that justifies them.
* Do not sacrifice accessibility or architecture for a performance gain.

---

# 19. Testing

Tooling is proposed here; confirm before installing if the repo has no setup yet.

* **Unit / component:** Vitest + Testing Library for components, hooks, the block registry, and consent logic.
* **Accessibility:** automated axe checks on key templates.
* **End-to-end:** Playwright for critical flows: homepage render, menu (desktop and mobile), job search, event registration states, `/giris` choice page, consent banner and analytics gating, theme toggle, 404.
* **Contract:** a check that the generated API client is up to date with the OpenAPI document.
* **Static:** `tsc --noEmit`, ESLint, Prettier, all must pass.
* Test the unhappy paths: empty, error, slow, not found, and unknown block type.
* Do not suppress failing tests, skip them, or comment out broken code and call the task done.

---

# 20. Windows Server Deployment

Production is **Windows Server in Turkey (Turhost), running Node.js**. Docker is not used.

* The public website builds with Next.js `standalone` output and runs as a **Windows service**. The service wrapper (for example WinSW or NSSM) is a deployment decision to be documented, not assumed.
* IIS acts as the reverse proxy (URL Rewrite + ARR) and routes by the path map in section 9.
* **All scripts must work on Windows.** Write build/dev scripts as cross-platform Node scripts or `package.json` scripts. No bash-only scripts, no hardcoded `/` or `\` path assumptions (use `path`), mind line endings (`.gitattributes`).
* Configuration comes from environment variables. Never commit secrets. Document every variable.
* Provide a documented deployment procedure (build, copy, service restart, rollback). If it does not exist yet, create it as part of the first deployment task, not as an afterthought.

---

# 21. Forbidden Shortcuts

Never:

* Hardcode UI strings, colors, radii, or API URLs in components.
* Hand-write or hand-edit generated API types.
* Fake backend data in production code paths.
* Call `fetch` directly in components. Go through the API client.
* Put business rules, authorization decisions, or validation truth in the frontend.
* Read session/cookies in a public page render.
* Use `dangerouslySetInnerHTML` outside `SafeHtml`.
* Store tokens in `localStorage`/`sessionStorage`.
* Load any third-party script, font, or embed before consent or without a decision.
* Put several components in one file, or create `utils.ts`/`helpers.ts`/`common.ts` buckets.
* Add a dependency without the checks in section 16.
* Use `any` to silence the type checker (use `unknown` and narrow; justify exceptions in a comment).
* Disable lint/type rules to make a build pass.
* Skip empty, error, and loading states.
* Ignore `prefers-reduced-motion` or keyboard access.

---

# 22. Existing Violations

Existing violations do not authorize new ones. When a task touches one: identify it, fix it if reasonably in scope, preserve behavior, update references, run tests. Do not refactor the whole workspace without authorization. Report violations outside scope.

---

# 23. Claude Workflow (Claude Code and Others)

## Before implementation

```text
1. Read the root AGENTS.md and this file.
2. Read the decision documents and relevant ADRs.
3. Inspect the repository and the OpenAPI document. Do not assume existing code is correct.
4. State the plan (section 3) and the file list.
```

## During implementation

```text
1. Keep the change scoped.
2. Keep route files thin; keep features in their slice.
3. Use tokens, the dictionary, and the generated client.
4. Implement all UI states.
5. Follow one-component-per-file and naming rules.
6. Add or update tests.
```

## Before completion

```text
1. Review changed files; filenames match exports.
2. No hardcoded strings/colors; no direct fetch; no bucket files.
3. Public pages are session-independent.
4. Consent gating still works; no new third-party requests.
5. Accessibility and keyboard checked.
6. Run: typecheck, lint, format check, unit tests, build; run e2e where relevant.
7. Report exact results.
```

## Definition of done

```text
[ ] Correct app, route, and slice
[ ] Data comes from the generated client; no invented contract
[ ] All UI states handled (loading, empty, error, not found)
[ ] Tokens and dictionary used; no hardcoded values
[ ] Public page is cacheable and session-independent
[ ] SEO metadata correct
[ ] Accessibility (keyboard, focus, contrast, reduced motion)
[ ] No unconsented third-party requests
[ ] Tests added/updated and passing
[ ] Typecheck, lint, format, build successful
[ ] One-component-per-file rule satisfied
[ ] No architecture shortcuts
```

If an item is not applicable, state why.

## Completion report

```text
Summary
- What changed and why.

Frontend
- App/route/slice affected.
- Endpoints and OpenAPI operations used.
- States covered; accessibility notes.
- Consent/analytics impact.

Validation
- Commands executed, build result, test result, lint/type result.

Backend gaps
- Missing or unclear contract items found.

Remaining issues
- Existing violations, deferred work, risks, assumptions.
```

Do not claim a command succeeded if it was not actually executed.

---

# 24. Final Principle

```text
Business requirements
        ↓
Backend contract (OpenAPI)
        ↓
Security and privacy (KVKK)
        ↓
Accessibility
        ↓
Architecture
        ↓
User experience and design
        ↓
Performance optimization
        ↓
Implementation convenience
```

Implementation convenience is always the lowest priority. The objective is a public site that is trustworthy, accessible, fast, easy to extend with new blocks, and honest about the data it shows.
