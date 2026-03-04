# Component Architecture Guide

## 🏗️ Component Hierarchy

```
App (with BrowserRouter)
└── AppRoutes
    └── MainLayout (Sidebar + Outlet)
        ├── DashboardPage
        │   └── Card (multiple)
        │       ├── CardHeader
        │       └── CardBody
        │
        ├── CategoryManagementPage
        │   ├── Card (grid)
        │   ├── Modal (list)
        │   │   ├── Input (search)
        │   │   ├── Button (add new)
        │   │   └── Table
        │   └── Modal (form)
        │       ├── Input (fields)
        │       └── Button (save/cancel)
        │
        └── CrewManagementPage
            ├── Input (search)
            ├── Select (filter)
            ├── Button (actions)
            ├── Card (grid)
            │   ├── CardBody
            │   └── CardFooter (action buttons)
            ├── Modal (form)
            │   ├── Input (multiple)
            │   ├── Select
            │   └── Button
            └── Modal (detail view)
```

---

## 📦 Component Categories

### 1. Layout Components (`components/layout/`)

#### MainLayout
- **Purpose**: Wrapper chính cho tất cả pages
- **Features**: Sidebar + main content area
- **Usage**:
  ```tsx
  <Route element={<MainLayout />}>
    <Route path="/dashboard" element={<DashboardPage />} />
  </Route>
  ```

#### Sidebar
- **Purpose**: Navigation menu
- **Features**: Active state, routing
- **Props**: `menuItems: MenuItem[]`

---

### 2. Common Components (`components/common/`)

#### Button
- **Variants**: primary, secondary, ghost
- **Sizes**: sm, md, lg
- **Props**:
  ```tsx
  interface ButtonProps {
    variant?: 'primary' | 'secondary' | 'ghost';
    size?: 'sm' | 'md' | 'lg';
    fullWidth?: boolean;
    children: React.ReactNode;
  }
  ```
- **Usage**:
  ```tsx
  <Button variant="primary" onClick={handleClick}>
    Save
  </Button>
  ```

#### Card
- **Sub-components**: CardHeader, CardBody, CardFooter
- **Props**:
  ```tsx
  interface CardProps {
    children: React.ReactNode;
    className?: string;
    onClick?: () => void;
    hoverable?: boolean;
  }
  ```
- **Usage**:
  ```tsx
  <Card hoverable>
    <CardHeader>
      <h3>Title</h3>
      <Badge>5</Badge>
    </CardHeader>
    <CardBody>
      Content here
    </CardBody>
    <CardFooter>
      <Button>Action</Button>
    </CardFooter>
  </Card>
  ```

#### Modal
- **Sizes**: sm, md, lg, xl
- **Features**: ESC to close, click outside to close, focus trap
- **Props**:
  ```tsx
  interface ModalProps {
    isOpen: boolean;
    onClose: () => void;
    title?: string;
    children: React.ReactNode;
    size?: 'sm' | 'md' | 'lg' | 'xl';
  }
  ```
- **Usage**:
  ```tsx
  const [isOpen, setIsOpen] = useState(false);
  
  <Modal
    isOpen={isOpen}
    onClose={() => setIsOpen(false)}
    title="Add New Item"
    size="lg"
  >
    <form>...</form>
  </Modal>
  ```

#### Input
- **Features**: Label, error display, full width option
- **Props**:
  ```tsx
  interface InputProps {
    label?: string;
    error?: string;
    fullWidth?: boolean;
    // + all native input props
  }
  ```
- **Usage**:
  ```tsx
  <Input
    label="Name"
    value={name}
    onChange={(e) => setName(e.target.value)}
    error={errors.name}
    required
    fullWidth
  />
  ```

#### Select
- **Features**: Label, error display, options array
- **Props**:
  ```tsx
  interface SelectProps {
    label?: string;
    error?: string;
    fullWidth?: boolean;
    options: Array<{ value: string | number; label: string }>;
  }
  ```
- **Usage**:
  ```tsx
  <Select
    label="Position"
    value={position}
    onChange={(e) => setPosition(e.target.value)}
    options={[
      { value: '', label: 'Select...' },
      { value: 'captain', label: 'Captain' },
      { value: 'engineer', label: 'Engineer' },
    ]}
    fullWidth
  />
  ```

---

## 🎨 Styling Strategy

### CSS Variables (Design Tokens)
All components use CSS variables from `styles/variables.css`:

```css
/* Colors */
--color-primary
--color-surface
--color-background
--color-border
--color-text

/* Spacing */
--spacing-xs/sm/md/lg/xl

/* Typography */
--font-size-xs/sm/md/lg/xl/2xl/3xl

/* Effects */
--shadow-sm/md/lg
--transition-fast/normal/slow
```

### CSS File Organization
Each component has its own CSS file:
```
Button/
  Button.tsx       # Component logic
  Button.css       # Component styles
  index.ts         # Barrel export
```

### CSS Class Naming
- Use BEM-inspired naming
- Prefix with component name
- Example: `.btn`, `.btn-primary`, `.btn-sm`

---

## 🔄 Data Flow Patterns

### 1. Page Level State (Current)
```tsx
// CategoryManagementPage.tsx
const [data, setData] = useState<Category[]>(mockData);
const [isModalOpen, setIsModalOpen] = useState(false);
```

### 2. Future: Service Layer
```tsx
// services/category.service.ts
export const CategoryService = {
  list: () => api.get<Category[]>('/categories'),
  create: (data: Category) => api.post('/categories', data),
  update: (id: string, data: Category) => api.put(`/categories/${id}`, data),
  delete: (id: string) => api.delete(`/categories/${id}`),
};

// Page
const { data, isLoading, error } = useCategoryList();
```

### 3. Future: Store (Zustand)
```tsx
// store/useCategoryStore.ts
interface CategoryStore {
  categories: Category[];
  fetchCategories: () => Promise<void>;
  addCategory: (category: Category) => void;
}

// Page
const { categories, addCategory } = useCategoryStore();
```

---

## 📐 Component Design Principles

### 1. Single Responsibility
Each component does ONE thing well:
- ✅ `Button` - renders a button
- ✅ `Card` - displays content in a card
- ❌ `ButtonCardWithForm` - too complex!

### 2. Composition over Configuration
Prefer:
```tsx
<Card>
  <CardHeader><h3>Title</h3></CardHeader>
  <CardBody>Content</CardBody>
</Card>
```

Over:
```tsx
<Card title="Title" body="Content" />
```

### 3. Controlled Components
Input components should be controlled:
```tsx
// ✅ Good - Controlled
<Input value={name} onChange={handleChange} />

// ❌ Bad - Uncontrolled
<input defaultValue={name} />
```

### 4. TypeScript First
Always define interfaces:
```tsx
interface Props {
  title: string;
  onSave: (data: FormData) => void;
}

export const MyComponent: React.FC<Props> = ({ title, onSave }) => {
  // ...
}
```

### 5. Separation of Concerns
```
Component.tsx     → Logic & JSX
Component.css     → Styles
Component.types.ts → Types (if complex)
Component.test.tsx → Tests (future)
index.ts          → Exports
```

---

## 🔌 Integration Patterns

### Modal + Form Pattern
```tsx
const [isOpen, setIsOpen] = useState(false);
const [formData, setFormData] = useState(initialData);

const handleSubmit = (e: React.FormEvent) => {
  e.preventDefault();
  // Save logic
  setIsOpen(false);
};

return (
  <>
    <Button onClick={() => setIsOpen(true)}>Add New</Button>
    
    <Modal isOpen={isOpen} onClose={() => setIsOpen(false)}>
      <form onSubmit={handleSubmit}>
        <Input
          label="Name"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
        />
        <Button type="submit">Save</Button>
      </form>
    </Modal>
  </>
);
```

### Search + Filter Pattern
```tsx
const [search, setSearch] = useState('');
const [filter, setFilter] = useState('');

const filteredData = data.filter(item => {
  const matchSearch = item.name.toLowerCase().includes(search.toLowerCase());
  const matchFilter = !filter || item.category === filter;
  return matchSearch && matchFilter;
});

return (
  <>
    <Input
      placeholder="Search..."
      value={search}
      onChange={(e) => setSearch(e.target.value)}
    />
    <Select
      value={filter}
      onChange={(e) => setFilter(e.target.value)}
      options={filterOptions}
    />
    <Grid data={filteredData} />
  </>
);
```

### Grid + Card Pattern
```tsx
<div className="grid">
  {items.map(item => (
    <Card key={item.id} hoverable>
      <CardBody>
        <h3>{item.name}</h3>
        <p>{item.description}</p>
      </CardBody>
      <CardFooter>
        <Button onClick={() => handleEdit(item)}>Edit</Button>
        <Button onClick={() => handleDelete(item.id)}>Delete</Button>
      </CardFooter>
    </Card>
  ))}
</div>
```

---

## 🚀 Performance Best Practices

### 1. Memoization
```tsx
// Expensive computation
const filteredData = useMemo(
  () => data.filter(/* complex filter */),
  [data, filterValue]
);

// Event handlers
const handleClick = useCallback(() => {
  // handler logic
}, [dependencies]);
```

### 2. Lazy Loading
```tsx
// Future: Lazy load pages
const DashboardPage = lazy(() => import('./pages/Dashboard'));
```

### 3. Avoid Inline Functions
```tsx
// ❌ Bad - Creates new function every render
<Button onClick={() => handleClick(id)}>Click</Button>

// ✅ Good - Stable reference
const handleButtonClick = () => handleClick(id);
<Button onClick={handleButtonClick}>Click</Button>
```

---

## 📝 Future Enhancements

### Add to Component Library:

1. **Table Component** - Reusable data table
2. **Pagination** - For large lists
3. **Toast/Notification** - User feedback
4. **Loading Spinner** - Loading states
5. **Empty State** - When no data
6. **Confirmation Dialog** - Before delete
7. **Form Components** - Textarea, Checkbox, Radio
8. **Date Picker** - Date input
9. **File Upload** - With preview
10. **Badge/Tag** - Status indicators

---

## 🎯 Component Checklist

When creating new component:

- [ ] Create folder in `components/common/`
- [ ] Create `ComponentName.tsx`
- [ ] Create `ComponentName.css`
- [ ] Create `index.ts` with exports
- [ ] Define TypeScript interface
- [ ] Use CSS variables for styling
- [ ] Make it responsive
- [ ] Add to `components/common/index.ts`
- [ ] Document props & usage
- [ ] Test in Storybook (future)

---

**Tip**: Start with existing components as templates!
