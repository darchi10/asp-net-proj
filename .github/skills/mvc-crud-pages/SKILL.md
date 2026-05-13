---
name: mvc-crud-pages
description: >
  Create or edit ASP.NET MVC CRUD pages and controller actions for any entity.
    Use this skill whenever the user wants to add a Create, Edit, Details, Index, or Delete
    page/option, wire up controller actions, scaffold a new form, update an existing view,
    fix a CRUD workflow, or add routing for any model in an ASP.NET MVC project — even if
    they don't say "CRUD" explicitly. Trigger on phrases like "add a create page", "build an
    edit form", "add a delete option", "scaffold a controller", "add a details view",
    "hook up the form", or "I need a page to manage [entity]".
argument-hint: 'Entity name, controller name, page type (Create/Edit/Details/Index/Delete), data source (repository/DbContext)'
---

# MVC CRUD Pages Skill

## What This Produces
- Controller actions (GET + POST where needed) for Create, Edit, Delete, and/or Index
- Razor view pages (`.cshtml`) matching those actions
- Correct model binding, validation, and routing
- Delete dependency checking with precomputed dependency status
- Soft delete support (`IsDeleted`, `DeletedAt`) when dependencies exist
- Hard delete cascade removal of related entities when dependencies exist
- Consistent patterns matching whatever the existing codebase uses

## Inputs
- Entity name and model type (e.g., `Phone`)
- Controller name (e.g., `PhonesController`)
- Page type: Create, Edit, Details, Index, Delete (or multiple)
- Data source: repository pattern or DbContext — check existing controllers first

---

## Procedure

1. **Read the codebase first.** Find an existing controller and its views to understand the patterns in use (repository vs. DbContext, layout, naming, validation style).
2. **Add controller action(s)** following the patterns below.
3. **Add or update the view** in `Views/<Controller>/<Action>.cshtml`.
4. **Verify routing** matches default MVC conventions.
5. **Check navigation links** (back to Index, to Details/Edit/Delete as needed).
6. **Run a build check** to confirm no new errors or warnings.

---

## Project-Specific Defaults (MobilePhoneServiceAndSalesSystem)

- Data access: use `AppDbContext` directly (no repository pattern).
- Routing: follow existing attribute routes like `[Route("phones")]` and `[Route("edit/{id:int}")]`.
- Views: use existing Bootstrap-heavy card layouts and styling; keep structure consistent.
- Delete: **always precompute dependencies on GET** and show a confirmation view that already knows if dependencies exist.
    - If dependencies exist: show **Soft Delete** + **Hard Delete** options.
    - If no dependencies: show only **Hard Delete**.
    - Soft delete = set `IsDeleted = true` and `DeletedAt = DateTime.UtcNow` on the entity.
    - Hard delete = remove the entity and **all related data** in a safe order.
    - Filter soft-deleted records out of all Index/Details queries and dropdowns.

### Entity Dependency Rules (project-specific)

Use these checks before delete:

- Customer: dependencies are Orders and Phones.
- Phone: block delete if there are any RepairJobs.
- Order: block delete if there are any OrderItems.
- Product: block delete if there are any OrderItems.
- Technician: block delete if there are any RepairJobs.
- SparePart: block delete if there are any RepairJobs.
- RepairJob: allow delete only if business rules allow (no confirmed restriction found in current code).

---

## Controller Patterns

### Create (GET + POST)
```csharp
// GET: /Phones/Create
public IActionResult Create()
{
    return View();
}

// POST: /Phones/Create
[HttpPost]
public IActionResult Create(Phone phone)
{
    if (ModelState.IsValid)
    {
        _repository.Add(phone); // or _context.Phones.Add(phone); _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
    return View(phone);
}
```

### Edit (GET + POST)
```csharp
// GET: /Phones/Edit/5
public IActionResult Edit(int id)
{
    var phone = _repository.GetById(id); // or _context.Phones.Find(id);
    if (phone == null) return NotFound();
    return View(phone);
}

// POST: /Phones/Edit/5
[HttpPost]
public IActionResult Edit(int id, Phone phone)
{
    if (id != phone.Id) return NotFound();

    if (ModelState.IsValid)
    {
        _repository.Update(phone); // or _context.Update(phone); _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
    return View(phone);
}
```

### Details (GET)
```csharp
// GET: /Phones/Details/5
public IActionResult Details(int id)
{
    var phone = _repository.GetById(id);
    if (phone == null) return NotFound();
    return View(phone);
}
```

### Index (GET)
```csharp
// GET: /Phones
public IActionResult Index()
{
    var phones = _repository.GetAll(); // or _context.Phones.ToList();
    return View(phones);
}
```

**Rules:**
- Always check `ModelState.IsValid` before persisting.
- Always return `NotFound()` when an entity lookup by id fails.
- Use the same data access pattern (repository or DbContext) found in existing controllers.

---

## Delete — Full Pattern (Project-Adjusted)

Delete is the most complex CRUD operation. Before implementing it, **always scan the DbContext or migrations to identify all FK relationships** pointing to the entity. In this project, the default rule is to **precompute dependency status and offer soft/hard delete choices when dependencies exist**.

---

### Step 1 — Classify the Delete Scenario

For this project:

- If any FK references exist, show a confirmation page that **already knows** dependencies exist.
- If there are no references, allow hard delete only.
- Soft delete is used when dependencies exist; add `IsDeleted`/`DeletedAt` to the model.

---

### Step 2 — Choose the Delete Strategy

#### Strategy A: Hard Delete Only (no dependencies)
Use when: entity has **no** FK references. Show only a hard delete button.

```csharp
// GET: /Phones/Delete/5
public IActionResult Delete(int id)
{
    var phone = _repository.GetById(id);
    if (phone == null) return NotFound();
    return View(phone);
}

// POST: /Phones/Delete/5
[HttpPost, ActionName("Delete")]
public IActionResult DeleteConfirmed(int id)
{
    var phone = _repository.GetById(id);
    if (phone == null) return NotFound();

    _repository.Delete(phone);
    return RedirectToAction(nameof(Index));
}
```

View: standard confirmation page (no modal needed).

---

#### Strategy B: Soft + Hard Delete Options (dependencies exist)
Use when: entity **has** FK references. Show both options and precompute dependency status in GET.

**Model must have:**
```csharp
public bool IsDeleted { get; set; }
public DateTime? DeletedAt { get; set; }
```

**Repository/DbContext — filter soft-deleted records globally:**
```csharp
// In repository GetAll / DbContext query filters:
.Where(x => !x.IsDeleted)
```

**Controller (pattern):**
```csharp
// GET: /Customers/Delete/5
[HttpGet]
public IActionResult Delete(int id)
{
    var customer = _dbContext.Customers
        .Include(c => c.Orders)
        .Include(c => c.Phones)
        .FirstOrDefault(c => c.Id == id && !c.IsDeleted);
    if (customer == null) return NotFound();

    ViewBag.HasDependencies = customer.Orders.Any() || customer.Phones.Any();
    return View(customer);
}

// POST: /Customers/Delete/5
[HttpPost, ActionName("Delete")]
public IActionResult DeleteConfirmed(int id, string deleteMode)
{
    var customer = _dbContext.Customers
        .Include(c => c.Orders)
        .ThenInclude(o => o.OrderItems)
        .Include(c => c.Phones)
        .ThenInclude(p => p.RepairJobs)
        .FirstOrDefault(c => c.Id == id && !c.IsDeleted);
    if (customer == null) return NotFound();

    var hasDependencies = customer.Orders.Any() || customer.Phones.Any();
    if (hasDependencies
        && !string.Equals(deleteMode, "soft", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(deleteMode, "hard", StringComparison.OrdinalIgnoreCase))
    {
        TempData["Error"] = "Choose a delete option for records with related data.";
        return RedirectToAction(nameof(Index));
    }

    if (string.Equals(deleteMode, "soft", StringComparison.OrdinalIgnoreCase))
    {
        customer.IsDeleted = true;
        customer.DeletedAt = DateTime.UtcNow;
        _dbContext.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    var orderItems = customer.Orders.SelectMany(o => o.OrderItems).ToList();
    if (orderItems.Any()) _dbContext.OrderItems.RemoveRange(orderItems);
    if (customer.Orders.Any()) _dbContext.Orders.RemoveRange(customer.Orders);

    var repairJobs = customer.Phones.SelectMany(p => p.RepairJobs).ToList();
    if (repairJobs.Any()) _dbContext.RepairJobs.RemoveRange(repairJobs);
    if (customer.Phones.Any()) _dbContext.Phones.RemoveRange(customer.Phones);

    _dbContext.Customers.Remove(customer);
    _dbContext.SaveChanges();

    return RedirectToAction(nameof(Index));
}
```

---

#### Strategy C: Block Delete (exception-only)
Use only if the business rules explicitly forbid any deletion for an entity. This is not the default in this project.

---

### Step 3 — Delete Views

#### Standard Delete Confirmation (no dependencies)
```html
@model Phone

<h2>Brisanje: @Model.Name</h2>
<p>Jeste li sigurni da želite obrisati ovaj zapis?</p>

<dl>
    <dt>@Html.DisplayNameFor(m => m.Name)</dt>
    <dd>@Html.DisplayFor(m => m.Name)</dd>
</dl>

<form asp-action="Delete">
    <input asp-for="Id" type="hidden" />
    <button type="submit" class="btn btn-danger">Obriši</button>
    <a asp-action="Index" class="btn btn-secondary">Odustani</a>
</form>
```

#### Dependency-Aware Delete Confirmation (dependencies exist)
```html
@model Customer

@if ((bool)(ViewBag.HasDependencies ?? false))
{
    <p>This record has related data. Choose soft or hard delete.</p>
}
else
{
    <p>This record can be permanently deleted.</p>
}

<form asp-action="Delete" asp-route-id="@Model.Id" method="post">
    @Html.AntiForgeryToken()
    <input asp-for="Id" type="hidden" />
    @if ((bool)(ViewBag.HasDependencies ?? false))
    {
        <button type="submit" name="deleteMode" value="soft">Soft Delete</button>
        <button type="submit" name="deleteMode" value="hard">Hard Delete</button>
    }
    else
    {
        <button type="submit" name="deleteMode" value="hard">Delete</button>
    }
    <a asp-action="Details" asp-route-id="@Model.Id">Cancel</a>
</form>
```

#### Optional: Delete Modal (advanced)
If you want a modal-based delete flow, keep it consistent with the existing Bootstrap styling and reuse the same dependency checks. Otherwise, use the basic confirmation view + `TempData["Error"]` approach.

---

## View Patterns

### Create View
```html
@model Phone

<h2>Create Phone</h2>
<form asp-action="Create">
    <div asp-validation-summary="ModelOnly"></div>

    <div>
        <label asp-for="Name"></label>
        <input asp-for="Name" />
        <span asp-validation-for="Name"></span>
    </div>

    <!-- repeat for each field -->

    <button type="submit">Create</button>
    <a asp-action="Index">Back to List</a>
</form>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

### Edit View
```html
@model Phone

<h2>Edit Phone</h2>
<form asp-action="Edit">
    <div asp-validation-summary="ModelOnly"></div>

    <!-- REQUIRED: hidden id field -->
    <input asp-for="Id" type="hidden" />

    <div>
        <label asp-for="Name"></label>
        <input asp-for="Name" />
        <span asp-validation-for="Name"></span>
    </div>

    <!-- repeat for each field -->

    <button type="submit">Save</button>
    <a asp-action="Index">Back to List</a>
</form>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

> ⚠️ Edit views **must** include `<input asp-for="Id" type="hidden" />`. Without it, the id is lost on POST and the record cannot be matched.

### Details View
```html
@model Phone

<h2>Details</h2>
<dl>
    <dt>@Html.DisplayNameFor(m => m.Name)</dt>
    <dd>@Html.DisplayFor(m => m.Name)</dd>
    <!-- repeat for each field -->
</dl>

<a asp-action="Edit" asp-route-id="@Model.Id">Edit</a>
<a asp-action="Delete" asp-route-id="@Model.Id">Delete</a>
<a asp-action="Index">Back to List</a>
```

### Index View
```html
@model IEnumerable<Phone>

<h2>Phones</h2>
<a asp-action="Create">Create New</a>

@if (TempData["Error"] != null)
{
    <div class="alert alert-danger">@TempData["Error"]</div>
}

<table>
    <thead>
        <tr>
            <th>@Html.DisplayNameFor(m => m.Name)</th>
            <th></th>
        </tr>
    </thead>
    <tbody>
    @foreach (var item in Model)
    {
        <tr>
            <td>@Html.DisplayFor(_ => item.Name)</td>
            <td>
                <a asp-action="Details" asp-route-id="@item.Id">Details</a>
                <a asp-action="Edit" asp-route-id="@item.Id">Edit</a>
                <button class="btn btn-sm btn-danger delete-btn" data-id="@item.Id">Obriši</button>
            </td>
        </tr>
    }
    </tbody>
</table>

@Html.AntiForgeryToken()

<!-- Delete modal -->
<div id="deleteModal" ...> ... </div>

@section Scripts {
    <!-- Delete modal JS -->
}
```

**Rules:**
- Match the shared layout and `_ViewImports.cshtml` already in the project.
- Always include `_ValidationScriptsPartial` in Create/Edit views.
- Always use `asp-for`, `asp-validation-for`, and `asp-validation-summary` tag helpers.
- Always use `asp-action` and `asp-route-id` for links — no hardcoded URLs.
- For Delete, always go through the dependency-aware confirmation page — never delete directly from a GET link.
- When soft delete is enabled, filter out deleted records from lists, details, and dropdowns.
- Always display `TempData["Error"]` on Index for blocked delete feedback.

---

## Delete Strategy Decision Tree

Use this to decide which strategy to implement before writing any code:

```
Does the entity have FK references from other tables?
│
├── NO → Strategy A: Hard Delete Only
│
└── YES → Strategy B: Soft + Hard Delete Options
    - Soft delete always available
    - Hard delete cascades all related data
```

---

## Dropdowns and Foreign Keys

When a field references another entity (e.g., `CategoryId`), pass a `SelectList` from the controller and render it in the view:

**Controller:**
```csharp
ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", phone.CategoryId);
```

**View:**
```html
<label asp-for="CategoryId"></label>
<select asp-for="CategoryId" asp-items="ViewBag.CategoryId">
    <option value="">-- Select --</option>
</select>
<span asp-validation-for="CategoryId"></span>
```

Follow whatever pattern existing views use for lookups/selects.

---

## Routing Rules
- This project uses attribute routing on controllers; mirror the existing route patterns.
- Parameter name must be `id` (not `phoneId` or similar) to match route templates.
- Use `asp-action` and `asp-route-id` tag helpers everywhere; no hardcoded path strings.

---

## Quality Checks
- [ ] Controller action names match view file names exactly.
- [ ] Edit view has `<input asp-for="Id" type="hidden" />`.
- [ ] Delete strategy follows project rule: block delete if any dependencies exist.
- [ ] Dependency checks cover all FK relationships for the entity.
- [ ] `TempData["Error"]` displayed on Index for blocked deletes.
- [ ] `@Html.AntiForgeryToken()` present on Index if modal JS is used.
- [ ] `NotFound()` returned when entity lookup fails in all Delete actions.
- [ ] Validation summary and field-level `asp-validation-for` present on Create/Edit views.
- [ ] `_ValidationScriptsPartial` rendered in Create/Edit views.
- [ ] Navigation links use `asp-route-id` and preserve `id` correctly.
- [ ] Data access follows the existing repository or DbContext pattern — not mixed.
- [ ] No new build warnings or errors.