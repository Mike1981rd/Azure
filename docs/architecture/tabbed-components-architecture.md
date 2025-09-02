# 📚 Arquitectura de Componentes con Pestañas y Estado Elevado

## 📋 Tabla de Contenidos
1. [Introducción](#introducción)
2. [Problema Resuelto](#problema-resuelto)
3. [Arquitectura de la Solución](#arquitectura-de-la-solución)
4. [Implementación Paso a Paso](#implementación-paso-a-paso)
5. [Ejemplo Completo: Módulo de Clientes](#ejemplo-completo-módulo-de-clientes)
6. [Patrones y Mejores Prácticas](#patrones-y-mejores-prácticas)
7. [Checklist de Implementación](#checklist-de-implementación)

---

## 🎯 Introducción

Esta documentación describe la arquitectura estándar para implementar componentes con pestañas en Next.js 14, utilizando el patrón de "Estado Elevado" (Lifted State) para garantizar la persistencia de datos al navegar entre pestañas.

### ¿Cuándo usar esta arquitectura?
- ✅ Páginas de detalle/edición con múltiples secciones
- ✅ Formularios complejos divididos en pasos
- ✅ Dashboards con vistas tabuladas
- ✅ Configuraciones con categorías

---

## 🔴 Problema Resuelto

### Problema Original
Cuando cada pestaña maneja su propio estado local con `useState`:
- **Los datos se pierden** al cambiar de pestaña
- Las pestañas se desmontan y remontan, perdiendo el estado
- No hay forma de guardar todos los datos de todas las pestañas de una vez

### Solución Implementada
**Patrón de Estado Elevado (Lifted State Pattern)**:
- El componente padre mantiene TODO el estado
- Las pestañas reciben datos y funciones como props
- Los datos persisten al navegar entre pestañas
- Se puede guardar todo el formulario desde cualquier lugar

---

## 🏗️ Arquitectura de la Solución

```
┌─────────────────────────────────────────────┐
│           Componente Padre                   │
│         (CustomerDetail.tsx)                 │
│                                              │
│  ┌────────────────────────────────────┐     │
│  │   Estado Centralizado (formData)    │     │
│  │  ┌──────────────────────────────┐  │     │
│  │  │ overview: OverviewFormData   │  │     │
│  │  │ security: SecurityFormData   │  │     │
│  │  │ address: AddressFormData     │  │     │
│  │  │ notifications: NotifFormData │  │     │
│  │  └──────────────────────────────┘  │     │
│  └────────────────────────────────────┘     │
│                                              │
│  ┌────────────────────────────────────┐     │
│  │     Funciones de Actualización     │     │
│  │  • updateOverviewData()            │     │
│  │  • updateSecurityData()            │     │
│  │  • updateAddressData()             │     │
│  │  • updateNotificationsData()       │     │
│  └────────────────────────────────────┘     │
└─────────────┬───────────────────────────────┘
              │
              ├──── Props ────┐
              │               │
    ┌─────────▼───┐  ┌────────▼──┐  ┌─────────▼───┐
    │   Tab 1     │  │   Tab 2   │  │    Tab 3    │
    │ (Overview)  │  │ (Security)│  │  (Address)  │
    │             │  │           │  │             │
    │ Props:      │  │ Props:    │  │ Props:      │
    │ • formData  │  │ • formData│  │ • formData  │
    │ • onChange  │  │ • onChange│  │ • onChange  │
    └─────────────┘  └───────────┘  └─────────────┘
```

---

## 📝 Implementación Paso a Paso

### Paso 1: Definir Interfaces de Datos

```typescript
// CustomerDetail.tsx

// Interface para cada sección del formulario
export interface OverviewFormData {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  // ... más campos
}

export interface SecurityFormData {
  currentPassword: string;
  newPassword: string;
  isTwoFactorEnabled: boolean;
  // ... más campos
}

export interface AddressBillingFormData {
  addresses: Array<{
    type: string;
    addressLine1: string;
    addressLine2: string;
    city: string;
    state: string;
    postalCode: string;
    country: string;
    isDefault: boolean;
  }>;
  paymentMethods: Array<{
    type: string;
    last4: string;
    expiryMonth: number;
    expiryYear: number;
    isDefault: boolean;
  }>;
  billingPreferences: {
    invoiceEmail: string;
    autoCharge: boolean;
    paperlessBilling: boolean;
  };
}

// Interface combinada
interface AllFormData {
  overview: OverviewFormData;
  security: SecurityFormData;
  addressBilling: AddressBillingFormData;
  notifications: NotificationsFormData;
}
```

### Paso 2: Crear Estado Centralizado en el Padre

```typescript
export default function CustomerDetail({ customerId }: Props) {
  // Estado centralizado que persiste entre pestañas
  const [formData, setFormData] = useState<AllFormData>({
    overview: {
      firstName: '',
      lastName: '',
      email: '',
      // ... valores iniciales
    },
    security: {
      currentPassword: '',
      newPassword: '',
      isTwoFactorEnabled: false,
      // ... valores iniciales
    },
    addressBilling: {
      addresses: [],
      paymentMethods: [],
      billingPreferences: {
        invoiceEmail: '',
        autoCharge: false,
        paperlessBilling: true
      }
    },
    notifications: {
      // ... valores iniciales
    }
  });

  // Funciones para actualizar cada sección
  const updateOverviewData = (data: Partial<OverviewFormData>) => {
    setFormData(prev => ({
      ...prev,
      overview: { ...prev.overview, ...data }
    }));
  };

  const updateSecurityData = (data: Partial<SecurityFormData>) => {
    setFormData(prev => ({
      ...prev,
      security: { ...prev.security, ...data }
    }));
  };

  // ... más funciones de actualización
}
```

### Paso 3: Configurar el Sistema de Pestañas

```typescript
const tabs = [
  { id: 'overview', label: t('tabs.overview'), icon: '📊' },
  { id: 'security', label: t('tabs.security'), icon: '🔒' },
  { id: 'address', label: t('tabs.address'), icon: '📍' },
  { id: 'notifications', label: t('tabs.notifications'), icon: '🔔' }
];

const [activeTab, setActiveTab] = useState('overview');
```

### Paso 4: Renderizado Condicional con Props

```typescript
{/* Tab Content */}
<div className="tab-content">
  {activeTab === 'overview' && (
    <CustomerOverviewTab 
      customer={customer}
      formData={formData.overview}
      onFormChange={updateOverviewData}
      primaryColor={primaryColor}
      isEditing={isEditing}
      setIsEditing={setIsEditing}
    />
  )}
  
  {activeTab === 'security' && (
    <CustomerSecurityTab 
      customer={customer}
      formData={formData.security}
      onFormChange={updateSecurityData}
      primaryColor={primaryColor}
      isEditing={isEditing}
      setIsEditing={setIsEditing}
    />
  )}
  
  {/* ... más pestañas */}
</div>
```

### Paso 5: Implementar Componentes de Pestaña

```typescript
// CustomerOverviewTab.tsx

interface CustomerOverviewTabProps {
  customer: CustomerDetailDto | null;
  formData: OverviewFormData;  // Recibe datos como prop
  onFormChange: (data: Partial<OverviewFormData>) => void;  // Función para actualizar
  primaryColor: string;
  isEditing: boolean;
  setIsEditing: (value: boolean) => void;
}

export default function CustomerOverviewTab({ 
  customer, 
  formData,      // NO usar useState local
  onFormChange,  // Usar esta función para actualizar
  primaryColor,
  isEditing,
  setIsEditing
}: CustomerOverviewTabProps) {
  
  // NO hacer esto:
  // const [localData, setLocalData] = useState({...}); ❌
  
  // SÍ hacer esto:
  const handleInputChange = (field: string, value: any) => {
    onFormChange({ [field]: value }); ✅
  };

  return (
    <div>
      <input
        value={formData.firstName}  // Usar datos de props
        onChange={(e) => onFormChange({ firstName: e.target.value })}
      />
      {/* ... más campos */}
    </div>
  );
}
```

### Paso 6: Guardar Todos los Datos

```typescript
// En el componente padre
const handleSaveAll = async () => {
  try {
    // Combinar todos los datos de todas las pestañas
    const dataToSave = {
      ...formData.overview,
      ...formData.security,
      addresses: formData.addressBilling.addresses,
      paymentMethods: formData.addressBilling.paymentMethods,
      notificationPreferences: formData.notifications
    };
    
    if (isNewRecord) {
      await api.create(dataToSave);
    } else {
      await api.update(id, dataToSave);
    }
  } catch (error) {
    console.error('Error saving:', error);
  }
};
```

---

## 💼 Ejemplo Completo: Módulo de Clientes

### Estructura de Archivos
```
src/
├── app/dashboard/clientes/[id]/
│   └── page.tsx                    # Página que renderiza CustomerDetail
├── components/clientes/
│   ├── CustomerDetail.tsx          # Componente padre con estado
│   └── tabs/
│       ├── CustomerOverviewTab.tsx # Pestaña 1 (sin estado local)
│       ├── CustomerSecurityTab.tsx # Pestaña 2 (sin estado local)
│       ├── CustomerAddressBillingTab.tsx # Pestaña 3 (sin estado local)
│       └── CustomerNotificationsTab.tsx  # Pestaña 4 (sin estado local)
```

### CustomerDetail.tsx (Componente Padre)
```typescript
'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useI18n } from '@/contexts/I18nContext';
import CustomerOverviewTab from './tabs/CustomerOverviewTab';
import CustomerSecurityTab from './tabs/CustomerSecurityTab';
// ... más imports

export interface OverviewFormData {
  firstName: string;
  lastName: string;
  email: string;
  // ... más campos
}

export default function CustomerDetail({ customerId }: Props) {
  const { t } = useI18n();
  const [activeTab, setActiveTab] = useState('overview');
  const [primaryColor, setPrimaryColor] = useState('#22c55e');
  
  // ESTADO CENTRALIZADO
  const [formData, setFormData] = useState<AllFormData>({
    overview: { /* ... */ },
    security: { /* ... */ },
    addressBilling: { /* ... */ },
    notifications: { /* ... */ }
  });

  // FUNCIONES DE ACTUALIZACIÓN
  const updateOverviewData = (data: Partial<OverviewFormData>) => {
    setFormData(prev => ({
      ...prev,
      overview: { ...prev.overview, ...data }
    }));
  };

  // ... más funciones

  // GUARDAR TODO
  const handleSaveAll = async () => {
    const dataToSave = {
      ...formData.overview,
      ...formData.security,
      // ... combinar todos los datos
    };
    await api.save(dataToSave);
  };

  return (
    <div>
      {/* Navegación de pestañas */}
      <div className="tabs">
        {tabs.map(tab => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={activeTab === tab.id ? 'active' : ''}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Contenido de pestañas */}
      {activeTab === 'overview' && (
        <CustomerOverviewTab 
          formData={formData.overview}
          onFormChange={updateOverviewData}
          // ... más props
        />
      )}
      {/* ... más pestañas */}
    </div>
  );
}
```

### CustomerOverviewTab.tsx (Componente Hijo)
```typescript
'use client';

import React from 'react';
import { OverviewFormData } from '../CustomerDetail';

interface Props {
  formData: OverviewFormData;
  onFormChange: (data: Partial<OverviewFormData>) => void;
  primaryColor: string;
  isEditing: boolean;
  setIsEditing: (value: boolean) => void;
}

export default function CustomerOverviewTab({ 
  formData,     // Datos vienen del padre
  onFormChange, // Función para actualizar viene del padre
  primaryColor,
  isEditing,
  setIsEditing
}: Props) {
  // NO usar useState para datos del formulario
  // SÍ se puede usar useState para UI local (modales, dropdowns, etc.)
  const [showModal, setShowModal] = useState(false); // ✅ OK para UI

  return (
    <div className="p-6">
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label>First Name</label>
          <input
            value={formData.firstName}
            onChange={(e) => onFormChange({ firstName: e.target.value })}
            className="..."
          />
        </div>
        
        <div>
          <label>Email</label>
          <input
            value={formData.email}
            onChange={(e) => onFormChange({ email: e.target.value })}
            className="..."
          />
        </div>
      </div>
    </div>
  );
}
```

---

## 🎨 Patrones y Mejores Prácticas

### ✅ DO's (Hacer)

1. **Definir interfaces TypeScript claras**
   ```typescript
   export interface TabFormData {
     field1: string;
     field2: number;
     // Exportar para que las pestañas las puedan importar
   }
   ```

2. **Mantener el estado en el componente padre más cercano**
   ```typescript
   const [formData, setFormData] = useState<AllFormData>(initialState);
   ```

3. **Usar funciones específicas para actualizar cada sección**
   ```typescript
   const updateSectionData = (data: Partial<SectionFormData>) => {
     setFormData(prev => ({
       ...prev,
       section: { ...prev.section, ...data }
     }));
   };
   ```

4. **Inicializar datos cuando se carga un registro existente**
   ```typescript
   useEffect(() => {
     if (existingData) {
       setFormData({
         overview: mapToOverviewData(existingData),
         security: mapToSecurityData(existingData),
         // ...
       });
     }
   }, [existingData]);
   ```

5. **Separar estado de UI del estado de datos**
   ```typescript
   // En componente hijo
   const [showDropdown, setShowDropdown] = useState(false); // ✅ UI local
   // formData viene de props // ✅ Datos del formulario
   ```

### ❌ DON'Ts (No Hacer)

1. **NO usar estado local para datos del formulario en las pestañas**
   ```typescript
   // ❌ MALO - En componente de pestaña
   const [formData, setFormData] = useState({
     firstName: '',
     lastName: ''
   });
   ```

2. **NO pasar setters del useState directamente**
   ```typescript
   // ❌ MALO
   <TabComponent setFormData={setFormData} />
   
   // ✅ BUENO
   <TabComponent onFormChange={updateSpecificSection} />
   ```

3. **NO olvidar el cleanup en useEffect**
   ```typescript
   useEffect(() => {
     // Cargar datos
     return () => {
       // Cleanup si es necesario
     };
   }, []);
   ```

---

## 📱 Consideraciones de UI Responsive

### Mobile-First para Pestañas
```typescript
{/* Mobile: Stack vertical */}
<div className="sm:hidden">
  <div className="space-y-2">
    {tabs.map(tab => (
      <button className="w-full p-4 text-left">
        {tab.label}
      </button>
    ))}
  </div>
</div>

{/* Desktop: Tabs horizontales */}
<div className="hidden sm:flex border-b">
  {tabs.map(tab => (
    <button className="px-4 py-2 border-b-2">
      {tab.label}
    </button>
  ))}
</div>
```

### Botones de Acción
```typescript
{/* Mobile: Fixed bottom */}
<div className="fixed bottom-0 left-0 right-0 p-4 bg-white md:hidden">
  <button onClick={handleSave}>Save</button>
</div>

{/* Desktop: Inline */}
<div className="hidden md:flex justify-end mt-4">
  <button onClick={handleSave}>Save</button>
</div>
```

---

## ✅ Checklist de Implementación

### Preparación
- [ ] Identificar todas las secciones/pestañas necesarias
- [ ] Definir la estructura de datos para cada sección
- [ ] Crear interfaces TypeScript para type safety
- [ ] Determinar qué datos deben persistir entre pestañas

### Componente Padre
- [ ] Crear estado centralizado con todas las secciones
- [ ] Implementar funciones de actualización para cada sección
- [ ] Configurar el sistema de navegación de pestañas
- [ ] Implementar renderizado condicional de pestañas
- [ ] Crear función handleSaveAll que combine todos los datos
- [ ] Manejar carga de datos existentes (edición)
- [ ] Implementar manejo de errores

### Componentes Hijos (Pestañas)
- [ ] Definir Props interface con formData y onFormChange
- [ ] NO usar useState para datos del formulario
- [ ] Implementar todos los inputs usando formData de props
- [ ] Usar onFormChange para actualizar datos
- [ ] Separar estado de UI (modales, dropdowns) del estado de datos
- [ ] Implementar validaciones locales si son necesarias

### UI/UX
- [ ] Implementar diseño responsive para pestañas
- [ ] Agregar indicadores visuales de pestaña activa
- [ ] Incluir feedback de carga/guardado
- [ ] Implementar confirmación antes de salir con cambios sin guardar
- [ ] Agregar transiciones suaves entre pestañas

### Testing
- [ ] Verificar que los datos persisten al cambiar de pestaña
- [ ] Probar guardado con datos de todas las pestañas
- [ ] Verificar que la edición carga todos los datos correctamente
- [ ] Probar en móvil y desktop
- [ ] Verificar manejo de errores

### Documentación
- [ ] Documentar la estructura de datos
- [ ] Agregar comentarios en funciones clave
- [ ] Crear README con instrucciones de uso
- [ ] Documentar cualquier decisión arquitectónica especial

---

## 🚀 Plantilla de Inicio Rápido

```typescript
// ParentComponent.tsx
import React, { useState } from 'react';

// 1. Definir interfaces
export interface Tab1Data {
  field1: string;
  field2: number;
}

export interface Tab2Data {
  field3: boolean;
  field4: string[];
}

interface AllData {
  tab1: Tab1Data;
  tab2: Tab2Data;
}

export default function ParentComponent() {
  // 2. Estado centralizado
  const [formData, setFormData] = useState<AllData>({
    tab1: { field1: '', field2: 0 },
    tab2: { field3: false, field4: [] }
  });

  // 3. Funciones de actualización
  const updateTab1 = (data: Partial<Tab1Data>) => {
    setFormData(prev => ({
      ...prev,
      tab1: { ...prev.tab1, ...data }
    }));
  };

  const updateTab2 = (data: Partial<Tab2Data>) => {
    setFormData(prev => ({
      ...prev,
      tab2: { ...prev.tab2, ...data }
    }));
  };

  // 4. Renderizado
  return (
    <div>
      {activeTab === 'tab1' && (
        <Tab1Component 
          formData={formData.tab1}
          onFormChange={updateTab1}
        />
      )}
    </div>
  );
}

// Tab1Component.tsx
interface Props {
  formData: Tab1Data;
  onFormChange: (data: Partial<Tab1Data>) => void;
}

export default function Tab1Component({ formData, onFormChange }: Props) {
  return (
    <input
      value={formData.field1}
      onChange={(e) => onFormChange({ field1: e.target.value })}
    />
  );
}
```

---

## 📚 Referencias

- [React Docs: Lifting State Up](https://react.dev/learn/sharing-state-between-components)
- [Next.js 14 App Router](https://nextjs.org/docs/app)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)

---

## 🔄 Historial de Cambios

| Fecha | Versión | Cambios |
|-------|---------|---------|
| 2025-01-08 | 1.0 | Documentación inicial basada en módulo de Clientes |

---

## 📧 Contacto

Para preguntas sobre esta arquitectura, consultar el archivo CLAUDE.md o revisar la implementación de referencia en `/src/components/clientes/`.