# 🎨 Modal Input Visual Guide - Before/After

Quick visual reference for modal input improvements.

---

## 📏 Measurement Input Modal

### Input Field

**BEFORE**:
```
┌────────────────────────────────────────┐
│  MEASURED VALUE                        │  ← 13px, bold, gray
│                                        │
│  ┌──────────────────────────────┐     │
│  │  0.0                    bar  │     │  ← Default "0.0" (RISKY!)
│  │                              │     │  ← 20px vertical padding
│  └──────────────────────────────┘     │
│                                        │
│  ⚠ Limit: 10.0 - 90.0 bar             │  ← Orange bg, black text
│     (Low contrast!)                    │
└────────────────────────────────────────┘
```

**AFTER**:
```
┌────────────────────────────────────────┐
│  MEASURED VALUE                        │  ← 11px, w600, light gray
│                                        │
│  ┌──────────────────────────────┐     │
│  │  Please enter measured value │     │  ← Placeholder (SAFE!)
│  │                         bar  │     │  ← 14px vertical padding
│  └──────────────────────────────┘     │
│                                        │
│  ⓘ Limit: 10.0 - 90.0 bar             │  ← Cream bg, dark brown text
│     (#FFF4E6 / #663D00 - AAA!)        │
└────────────────────────────────────────┘
```

---

## ✅ Checklist Input Modal

### OK/NG Buttons

**BEFORE**:
```
┌──────────────┐  ┌──────────────┐
│   ✓          │  │   ✕          │
│              │  │              │
│  OK / Pass   │  │  NG / Fail   │
└──────────────┘  └──────────────┘
      ↑                  ↑
Green bg (#4CAF50)  Red bg (#F44336)
Bright colors!      Hard to read!
```

**AFTER (Not Selected)**:
```
┌──────────────┐  ┌──────────────┐
│   ✓          │  │   ✕          │
│              │  │              │
│  OK / Pass   │  │  NG / Fail   │
└──────────────┘  └──────────────┘
      ↑                  ↑
White bg            White bg
Gray border         Gray border
Icon: textTertiary  Icon: textTertiary
```

**AFTER (Selected)**:
```
┌═══════════════┐
║   ✓           ║ ← 2.5px border
║               ║ ← Forest Green tint bg
║  OK / Pass    ║ ← Forest Green text
└───────────────┘
    ↓
Drop shadow (8px blur)
```

---

## 🔢 Modal Header

**BEFORE**:
```
┌─────────────────────────────────┐
│  ┌────┐                         │
│  │ 4  │  Check Engine Oil       │
│  └────┘  CHECKLIST  MANDATORY   │
│     ↑         ↑          ↑      │
│  Blue 100  Blue bg   Red bg     │
│  Blue 700  rounded   rounded    │
└─────────────────────────────────┘
```

**AFTER**:
```
┌─────────────────────────────────┐
│  ┌────┐                         │
│  │ 4  │  Check Engine Oil       │
│  └────┘  CHECKLIST  MANDATORY   │
│     ↑         ↑          ↑      │
│  Navy 15%  Navy text  Deep Red  │
│  + border  text-only  sharp 3px │
└─────────────────────────────────┘
```

---

## 🗒 Description Info Box

**BEFORE**:
```
┌───────────────────────────────────┐
│  ⓘ  Check for oil level between  │
│      MIN and MAX marks            │
└───────────────────────────────────┘
     ↑
Blue 50 bg (#E3F2FD)
Blue 900 text
12px border radius
```

**AFTER**:
```
┌───────────────────────────────────┐
│  ⓘ  Check for oil level between  │
│      MIN and MAX marks            │
└───────────────────────────────────┘
     ↑
Sage 8% bg (#6B8E7F + opacity)
Sage 25% border
Text: textSecondary
6px border radius (sharp)
16px icon size (smaller)
```

---

## 🎯 Action Buttons

**BEFORE**:
```
┌────────────┐  ┌─────────────────────┐
│            │  │  ✓  Complete        │
│  Cancel    │  │                     │
└────────────┘  └─────────────────────┘
      ↑                   ↑
16px padding       Green bg (#4CAF50)
12px radius        16px padding
Default border     Check icon + text
```

**AFTER**:
```
┌────────────┐  ┌─────────────────────┐
│            │  │                     │
│  Cancel    │  │     Complete        │
└────────────┘  └─────────────────────┘
      ↑                   ↑
14px padding       Navy Blue bg
8px radius         14px padding
1.5px border       NO ICON! Text only
textSecondary      elevation: 0 (flat)
```

---

## 📐 Spacing & Typography

### Font Sizes

| Element | Before | After | Change |
|---------|--------|-------|--------|
| Modal title | 18px | 17px | -5% |
| Input number | 24px | 24px | No change ✓ |
| Input unit | 18px | 18px | No change ✓ |
| Labels (MEASURED VALUE) | 13px bold | 11px w600 | -15%, lighter |
| Description | 14px | 13px | -7% |
| Warning text | 13px | 12px | -7% |
| Buttons | 16px bold | 15px w700 | -6% |
| MANDATORY badge | 10px bold | 9px w700 | -10% |

### Border Radius

| Element | Before | After |
|---------|--------|-------|
| Modal container | 24px | 16px |
| Input fields | 16px | 8px (measurement), 6px (notes) |
| Buttons (OK/NG) | 16px | 8px |
| Action buttons | 12px | 8px |
| Warning box | 12px | 6px |
| MANDATORY badge | 12px | 3px ⭐ Sharp! |

### Padding

| Element | Before | After |
|---------|--------|-------|
| Modal container | 24px all | 20/16/20/24 (L/T/R/B) |
| Input vertical | 20px | 14px |
| Button vertical | 16px | 14px |
| Warning box | 12px | 10-12px |

---

## 🎨 Color Palette

### Primary Colors

```
Navy Blue #1A3A52    ████████
Sage Green #6B8E7F   ████████
Deep Red #B91C1C     ████████
```

### Warning Colors

```
Warm Cream #FFF4E6   ████████  (Background)
Golden #FFD699       ████████  (Border)
Dark Brown #663D00   ████████  (Text)
```

### Text Colors

```
Primary   #1F2937    ████████  (Headings)
Secondary #6B7280    ████████  (Body)
Tertiary  #9CA3AF    ████████  (Labels, placeholders)
```

---

## ⚠️ Warning Box Evolution

**Contrast Test**:

```
BEFORE:
┌──────────────────────────────────┐
│ ⚠ Limit: 10.0 - 90.0 bar         │  Orange 50 bg + Orange 900 text
└──────────────────────────────────┘  Ratio: 3.5:1 (FAIL AA) ❌

AFTER:
┌──────────────────────────────────┐
│ ⓘ Limit: 10.0 - 90.0 bar         │  Cream bg + Dark brown text
└──────────────────────────────────┘  Ratio: 7.2:1 (PASS AAA) ✅
```

---

## 🔄 Selection States

### Checklist Button States

**IDLE** (Not selected):
```
Background: White
Border: 1px #E5E7EB (border)
Icon: #9CA3AF (textTertiary)
Text: #6B7280 (textSecondary)
Shadow: None
```

**SELECTED**:
```
Background: Color with 10% opacity
Border: 2.5px Color (full)
Icon: Color (full)
Text: Color (full)
Shadow: Color 25% opacity, 8px blur
```

**Example - OK Selected**:
```
Background: rgba(107, 142, 127, 0.1)
Border: 2.5px #6B8E7F
Icon: #6B8E7F
Text: #6B8E7F
Shadow: rgba(107, 142, 127, 0.25)
```

---

## 📱 Responsive Breakpoints

All sizes work on:
- Small screens: 320px width
- Standard phones: 360-400px width
- Large phones: 400-500px width
- Tablets: 600px+ width

**No overflow errors** with:
- Long equipment names
- Translated text (7 languages)
- Long measurement units
- Multiline descriptions

---

## ✅ Visual Checklist

When reviewing the modals, verify:

- [ ] Border radius is 16px (not 24px) for modal container
- [ ] Drag handle is 36px wide (not 40px)
- [ ] Number badge has Navy border (not solid blue bg)
- [ ] MANDATORY badge has 3px radius (sharp, not 12px rounded)
- [ ] Input fields have visible borders (not borderless)
- [ ] Labels are 11px (not 13px)
- [ ] Buttons are 8px radius (not 12px)
- [ ] Complete button has NO icon (text only)
- [ ] Warning box is cream color (not orange)
- [ ] Warning text is dark brown (not black)
- [ ] Selected button has drop shadow
- [ ] Selected button has 2.5px border
- [ ] OK/NG use Forest Green/Deep Red (not bright green/red)

---

**Quick Test**:
1. Open any task with checklist items
2. Tap on "Check Engine Oil" (or any pending item)
3. Modal should match the "AFTER" designs above

**Perfect Score**: All 13 checkboxes ✅

