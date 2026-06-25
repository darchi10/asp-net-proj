# Responsive UI - Quick Guide

## 🎯 Kako testirati responsive design

### Chrome DevTools
1. Pritisni `Ctrl+Shift+M` (Windows) ili `Cmd+Opt+M` (Mac)
2. Odaberi device iz dropdown menija
3. Testiraj različite veličine ekrana

### Firefox
1. Pritisni `Ctrl+Shift+M` (Windows) ili `Cmd+Opt+M` (Mac)
2. Responsive Design Mode se otvara
3. Odaberi device ili custom dimenzije

## 📱 Breakpoints u projektu

```css
/* Mobile */
@media (max-width: 767px) { }

/* Tablet */
@media (min-width: 768px) and (max-width: 991px) { }

/* Desktop */
@media (min-width: 992px) { }

/* Large screens */
@media (min-width: 1400px) { }
```

## 🔧 Bootstrap Grid Classes

### Responsive Columns
```html
<!-- 1 col mobile, 2 cols tablet, 4 cols desktop -->
<div class="row row-cols-1 row-cols-md-2 row-cols-lg-4">
  <div class="col">Item 1</div>
  <div class="col">Item 2</div>
  <div class="col">Item 3</div>
  <div class="col">Item 4</div>
</div>
```

### Responsive Flex
```html
<!-- Stack on mobile, row on desktop -->
<div class="d-flex flex-column flex-md-row gap-3">
  <div>Item 1</div>
  <div>Item 2</div>
</div>
```

## ✨ Custom Utility Classes

### Page Header (Auto-responsive)
```html
<div class="page-header">
  <h2>Title</h2>
  <a href="#" class="btn btn-primary">Action</a>
</div>
```

### Action Buttons (Auto-responsive)
```html
<div class="action-buttons">
  <button class="btn btn-primary">Save</button>
  <button class="btn btn-secondary">Cancel</button>
</div>
```

### Responsive Card Grid
```html
<div class="responsive-card-grid">
  <div class="card">Card 1</div>
  <div class="card">Card 2</div>
  <div class="card">Card 3</div>
</div>
```

## 📝 Best Practices

### ✅ DO
- Koristi `flex-column` na mobile, `flex-row` na desktop
- Koristi Bootstrap grid classes (`col-md-6`, `col-lg-4`)
- Postavi `min-height: 44px` za touch targets
- Testiraj na realnim uređajima ako je moguće

### ❌ DON'T
- Ne koristi fixed widths za glavne containere
- Ne koristi `px` font sizes direktno (koristi `rem`)
- Ne ignoriraj touch devices (hover: none)
- Ne zaboravi testirati landscape orientation

## 🎨 Responsive Typography

```css
/* Font sizes automatski se skaliraju */
Mobile:  14px base
Tablet:  15px base
Desktop: 16px base
```

## 🚀 Quick Test Checklist

- [ ] Navbar se collapse-a na mobile
- [ ] Kartice se slažu u jednu kolonu na mobile
- [ ] Buttons su full-width na mobile
- [ ] Forme su vertikalne na mobile
- [ ] Touch targets su minimum 44x44px
- [ ] Tekst je čitljiv na malim ekranima
- [ ] Nema horizontal scrolling-a
- [ ] Footer je centriran na mobile

## 📊 Common Device Sizes

| Device | Width | Height |
|--------|-------|--------|
| iPhone SE | 375px | 667px |
| iPhone 12/13 | 390px | 844px |
| iPhone 14 Pro Max | 430px | 932px |
| iPad | 768px | 1024px |
| iPad Pro 12.9" | 1024px | 1366px |
| Desktop | 1920px | 1080px |

## 🔍 Debugging Tips

### Check if element is responsive
```javascript
// Console check
window.innerWidth < 768 ? 'Mobile' : 'Desktop'
```

### Force mobile view
```css
/* Add to site.css temporarily */
* { max-width: 375px !important; }
```

### View current breakpoint
```javascript
// Add to browser console
console.log('Breakpoint:', 
  window.innerWidth < 768 ? 'Mobile' :
  window.innerWidth < 992 ? 'Tablet' : 'Desktop'
);
```

## 💡 Hints

- `d-none d-md-block` - prikaži samo na desktop
- `d-block d-md-none` - prikaži samo na mobile
- `order-1 order-md-2` - promijeni redoslijed na desktop
- `text-center text-md-start` - centriraj na mobile, lijevo na desktop
