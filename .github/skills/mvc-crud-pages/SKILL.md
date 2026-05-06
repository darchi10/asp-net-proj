---
name: mvc-crud-pages
description: >
  Create or edit ASP.NET MVC CRUD pages and controller actions for any entity.
  Use this skill whenever the user wants to add a Create, Edit, Details, or Index page,
  wire up controller actions, scaffold a new form, update an existing view, fix a CRUD
  workflow, or add routing for any model in an ASP.NET MVC project — even if they don't
  say "CRUD" explicitly. Trigger on phrases like "add a create page", "build an edit form",
  "scaffold a controller", "add a details view", "hook up the form", or "I need a page to
  manage [entity]".
argument-hint: 'Entity name, controller name, page type (Create/Edit/Details/Index), data source (repository/DbContext)'
---

# MVC CRUD Pages Skill

## What This Produces
- Controller actions (GET + POST) for Create, Edit, Details, and/or Index
- Razor view pages (`.cshtml`) matching those actions
- Correct model binding, validation, and routing
- Consistent patterns matching whatever the existing codebase uses

## Inputs
- Entity name and model type (e.g., `Phone`)
- Controller name (e.g., `PhonesController`)
- Page type: Create, Edit, Details, Index (or multiple)
- Data source: repository pattern or DbContext — check existing controllers first

---

## Procedure

1. **Read the codebase first.** Find an existing controller and its views to understand the patterns in use (repository vs. DbContext, layout, naming, validation style).
2. **Add controller action(s)** following the patterns below.
3. **Add or update the view** in `Views/<Controller>/<Action>.cshtml`.
4. **Verify routing** matches default MVC conventions.
5. **Check navigation links** (back to Index, to Details/Edit as needed).
6. **Run a build check** to confirm no new errors or warnings.

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
<a asp-action="Index">Back to List</a>
```

### Index View
```html
@model IEnumerable<Phone>

<h2>Phones</h2>
<a asp-action="Create">Create New</a>

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
            </td>
        </tr>
    }
    </tbody>
</table>
```

**Rules:**
- Match the shared layout and `_ViewImports.cshtml` already in the project.
- Always include `_ValidationScriptsPartial` in Create/Edit views.
- Always use `asp-for`, `asp-validation-for`, and `asp-validation-summary` tag helpers.
- Always use `asp-action` and `asp-route-id` for links — no hardcoded URLs.

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
- Default convention: `/[Controller]/[Action]/{id?}` — do not add custom routes unless already present.
- Parameter name must be `id` (not `phoneId` or similar) to match default routing.
- Use `asp-action` and `asp-route-id` tag helpers everywhere; no hardcoded path strings.

---

## Quality Checks
- [ ] Controller action names match view file names exactly.
- [ ] Edit view has `<input asp-for="Id" type="hidden" />`.
- [ ] `NotFound()` returned when entity lookup fails.
- [ ] Validation summary and field-level `asp-validation-for` present on Create/Edit views.
- [ ] `_ValidationScriptsPartial` rendered in Create/Edit views.
- [ ] Navigation links use `asp-route-id` and preserve `id` correctly.
- [ ] Data access follows the existing repository or DbContext pattern — not mixed.
- [ ] No new build warnings or errors.
