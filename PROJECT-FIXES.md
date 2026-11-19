# QuickDrop Fixes and UI Improvements

This document records the issues found and changes made during the QuickDrop
build, runtime, functionality, and responsive-UI work in this session.

## Summary

The work addressed missing page layout application, JavaScript-dependent page
visibility, mobile navigation behavior, overflow and responsiveness, inconsistent
demo-account data, admin dashboard runtime errors, and repeated card layouts that
did not share a responsive grid. A Python HTTP workflow test script was added
under `test-scripts/`.

The app built successfully with no warnings or errors after the final UI changes,
and the HTTP workflow suite passed after the grid conversion.

## Earlier Active-Tab, Login, and Data-Flow Review

The attached earlier project session ("CSS fix for active tab and system flaws",
November 19, 2026) found and fixed additional issues that are part of this
change history:

- Navigation links now identify the active route visually and expose the current
  page with `aria-current="page"`.
- The login password show/hide control was repositioned inside its password
  field. Its label and `aria-pressed` state are updated when toggled.
- Login email input is trimmed and compared case-insensitively.
- Anti-forgery protection was added to login, order placement, restaurant status
  updates, and driver actions; the corresponding forms emit anti-forgery tokens.
- Order placement validates quantity, delivery address, payment method, and
  coordinate ranges before persisting an order.
- Order tracking requires a signed-in user and checks that the user owns the
  order, unless the user is an admin.
- Restaurant and driver endpoints verify the caller's role. Their submitted
  status values are allowlisted; driver status updates additionally require
  that the order is assigned to that driver.
- Driver acceptance rejects orders that have already been assigned, avoiding
  duplicate assignment.
- Tracking links and login handling were corrected as part of this flow review.

That review's `dotnet build --no-restore` completed with zero warnings and
errors, and smoke checks returned HTTP 200 for the home and login pages.

## Initial Build and Ignore Rules

- Added the root `.gitignore` with .NET `bin/` and `obj/` outputs, local IDE and
  OS files, and SQLite shared-memory/WAL sidecar exclusions.
- Restored packages and built the .NET 8 application. The first build succeeded
  but reported a nullable warning in the home view; that warning was later
  resolved by guarding the model-backed restaurant and product lists.
- Started the MVC application and verified its home endpoint over HTTP. Startup
  initializes the SQLite database and runs the demo data seeder.

## Issues Found and Fixes

### Views initially rendered without the shared site layout

**Observed:** The home route returned view content without the document-level
`<html>` markup, navigation, footer, or stylesheet links. This made the page look
unstyled even though `/css/site.css` itself returned HTTP 200.

**Cause:** There was no `Views/_ViewStart.cshtml` selecting the shared layout.

**Fix:** Added `Views/_ViewStart.cshtml` to select `Views/Shared/_Layout.cshtml`.
The generated home response then included the shared document and stylesheet.

### Content could disappear when JavaScript did not complete

**Observed:** The page could remain a dark screen or hide content.

**Cause:** The stylesheet initially set the body and `.reveal` elements to
`opacity: 0`, relying on JavaScript to add classes that made them visible.

**Fix:** Changed the defaults so the page and reveal content are visible without
JavaScript. JavaScript still applies its progressive enhancements and animations.

### Shared header and mobile menu were clipped or hard to operate

**Observed:** On a narrow viewport, the header did not occupy the full viewport
width and the notification link could intercept clicks intended for the menu
button.

**Cause:** The fixed header had no explicit full-width sizing, and the nested
navigation used redundant sticky positioning. The mobile menu also lacked
explicit accessibility state and a bounded scroll area.

**Fixes:**

- Set the header and navigation to full width and removed redundant sticky
  positioning from the nested navigation.
- Added `aria-controls`, `aria-expanded`, and a button type to the menu toggle.
- Updated the menu script to synchronize open state and its accessible label,
  close the menu after link selection or Escape, and close it when returning to
  desktop width.
- Added a larger mobile touch target and a menu height limit with internal
  scrolling.
- Reduced the navigation height to 64px on desktop and 56px on mobile, and
  adjusted the main-content offset accordingly.

The menu was tested in the browser: it opens, exposes its links, updates
`aria-expanded`, and closes on Escape.

### Extra layout width and scrollbar chrome interfered with page sizing

**Observed:** Some pages were narrower than expected or had nested layout and
overflow problems; horizontal scroll areas also displayed scrollbar chrome.

**Cause:** The shared layout placed every view inside an extra Bootstrap
`.container`, while many views already supplied their own container. Several
wide tables and mobile-only scroll areas also retained visible scrollbar tracks.

**Fixes:**

- Removed the redundant shared container around `RenderBody()`.
- Standardized minimum widths and responsive sizing for grid children, cards,
  media, charts, and tables.
- Hid scrollbar visuals while retaining scroll behavior for the page and
  scrollable components such as tables, category navigation, and the mobile menu.

### Repeated restaurant and food cards did not share responsive grid behavior

**Observed:** Related card collections used separate Bootstrap row/column markup
and sizing rules, making card widths inconsistent as the viewport changed.

**Fix:** Converted the repeated restaurant and food listings to shared CSS Grid
wrappers using auto-fitting tracks with minimum card widths:

- Home restaurant cards and popular food cards in `Views/Home/Index.cshtml`.
- Customer dashboard restaurant and recommendation cards in
  `Views/Customer/Index.cshtml`.
- Restaurant-manager restaurant cards in `Views/Restaurant/Index.cshtml`.

The grid styles define consistent gaps, card stretching, minimum widths, and
smaller minimum track sizes on phones. Existing card content and links remain
unchanged.

### Responsive sizing and element positioning issues

**Observed:** Dashboard table columns and side panels overflowed; fixed-width
searches, image dimensions, nested form controls, and header content could crowd
or exceed their parent containers.

**Fixes:** Added responsive grid rules for admin insights, bounded chart sizing,
internal table scrolling, minimum-width protections for grid/card children,
responsive image sizes, wrapping for action groups and headings, and smaller
viewport adjustments for card padding and navigation content.

### Home view emitted a nullable-reference warning

**Observed:** The build warned that the home view might enumerate a null model.

**Fix:** The restaurant listing and popular-food projection now fall back to an
empty restaurant collection when the model is null. The final build completed
with zero warnings and zero errors.

## Functionality Issues Found and Fixes

### Demo account data differed between the running database and login page

**Observed:** The database contained demo accounts using `@quickdrop.com`, but
the login page and seed data described accounts using `@gmail.com`. Demo logins
failed against the existing database.

**Fix:** Updated startup seeding in `Data/DbSeeder.cs` to synchronize the demo
accounts by role, including existing records, and to seed restaurants and
products only when those records are absent. The login page's stray text was
also removed.

The account values themselves are intentionally not repeated here; the demo
login view remains the source for the development credentials.

### Admin dashboard failed on SQLite revenue aggregation

**Observed:** Opening the admin dashboard resulted in HTTP 500.

**Cause:** SQLite's EF Core provider cannot translate `Sum` over the decimal
revenue expression used by the dashboard query.

**Fix:** Load the decimal totals and sum them in application code. The admin
dashboard then rendered successfully.

### Admin dashboard driver activity failed during Razor rendering

**Observed:** After the revenue issue was fixed, the admin dashboard still
returned HTTP 500.

**Cause:** The view accessed `Name`, `Status`, and `Location` on dynamic data,
but the controller supplied an anonymous object whose property was named
`FullName`.

**Fix:** Added a concrete `AdminActivityViewModel` and used it for driver activity
items, avoiding dynamic property binding. Related admin metrics and dashboard
data were also populated explicitly.

### Some visible links did not reach implemented actions

**Observed:** The `/Customer/Track` link returned 404 without an order ID, the
admin sidebar linked to missing actions, and the login view linked to missing
registration and password-recovery actions.

**Fixes:**

- `/Customer/Track` now selects the signed-in customer's latest active order,
  redirects to its tracking page, or returns to the customer dashboard when
  there is no active order.
- Admin subsection links now resolve and redirect to the main admin dashboard.
  These are navigation placeholders; dedicated Customers, Drivers, Orders,
  Payments, Reports, and Settings pages have not been implemented.
- Register and Forgot links now show clear demo notices that those features are
  not enabled.

Order tracking also checks authentication and restricts tracking to the owning
customer or an admin.

## Test Coverage Added

Added `test-scripts/test_quickdrop.py`, a standard-library Python HTTP workflow
suite. It verifies:

- Home page and search response.
- Login, invalid credentials, and demo notices for register/forgot pages.
- Restaurant detail and static CSS/JavaScript assets.
- Customer login, dashboard, order placement, and order tracking.
- Restaurant-manager login, dashboard, and order status update.
- Driver login, dashboard, order acceptance, and status update.
- Admin login, dashboard, and admin sidebar destinations.

The script obtains anti-forgery tokens from the relevant forms before submitting
POST requests. Driver acceptance is tested before changing the status, using the
newly placed test order.

Run the suite against a running application with:

```bash
python3 test-scripts/test_quickdrop.py http://127.0.0.1:5000
```

## Validation Results

- `dotnet build --nologo`: succeeded with **0 warnings and 0 errors** after the
  final responsive-grid changes.
- `python3 test-scripts/test_quickdrop.py http://127.0.0.1:5000`: all tests
  passed after the grid conversion.
- `git diff --check` on the changed views, stylesheet, and test script: passed.
- Browser verification of the mobile menu: open/close state and accessible
  attributes behaved as expected.

## Scope and Limitations

- The project remains a demo platform. Register and password recovery are
  notices, not implemented account workflows.
- The admin sidebar destinations currently redirect to the admin dashboard
  rather than separate administration pages.
- Live maps continue to require a configured Google Maps API key.
- Test workflow actions create and update records in the configured SQLite
  database. Use a disposable development database when you want test runs that
  do not change persistent demo data.
