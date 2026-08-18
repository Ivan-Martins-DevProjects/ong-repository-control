# Frontend — Criação de Páginas e Componentes

## Estrutura de diretórios

```
src/app/
├── components/          → Componentes reutilizáveis (ex: sidebar)
│   └── nome/
│       ├── nome.ts
│       ├── nome.html
│       └── nome.css
├── modules/             → Módulos/páginas da aplicação
│   └── nome/
│       ├── nome-page.ts      → Componente principal (ou nome do componente)
│       ├── nome-page.html
│       ├── nome-page.css
│       ├── item.ts            → Interfaces/Types
│       ├── *.service.ts       → Serviços (DI, providedIn: 'root')
│       └── sub-componente.ts  → Subcomponentes com .html/.css próprios
├── app.ts
├── app.html
├── app.css
├── app.routes.ts
└── app.config.ts
```

## Convenções para criar uma nova página

### 1. Criar o módulo em `modules/`

```bash
mkdir src/app/modules/minha-pagina
```

### 2. Arquivos do componente

**minha-pagina-page.ts** — standalone component com DI via `inject()`:

```typescript
import { Component, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-minha-pagina-page',
  imports: [FormsModule],  // só o que for usado no template
  templateUrl: './minha-pagina-page.html',
  styleUrl: './minha-pagina-page.css'
})
export class MinhaPaginaPage {
  private readonly router = inject(Router);
  // signals para estado reativo
  protected readonly items = signal<string[]>([]);
  protected readonly loading = computed(() => this.items().length === 0);
}
```

**minha-pagina-page.html** — template com variáveis de template `@if`, `@for`:

```html
<div class="page">
  <h1>Título</h1>
  <div class="card">
    <!-- conteúdo -->
  </div>
</div>
```

**minha-pagina-page.css** — tema escuro (slate palette):

```css
:host { display: contents; }
/* usar .card, .badge, .btn* do styles.css global */
```

### 3. Registrar rota em `app.routes.ts`

```typescript
import { MinhaPaginaPage } from './modules/minha-pagina/minha-pagina-page';

export const routes: Routes = [
  // ...
  { path: 'minha-rota', component: MinhaPaginaPage },
];
```

### 4. Adicionar na Sidebar (se aplicável)

Editar `components/sidebar/sidebar.html`:

```html
<a routerLink="/minha-rota" routerLinkActive="active" class="nav-item">
  <span class="material-symbols-outlined">icon_name</span>
  <span>Nome Visível</span>
</a>
```

## Padrões de código

### DI (injeção de dependência)
- Usar `inject()` — NUNCA constructor injection
- `private readonly nome = inject(NomeService);`

### Signals (estado reativo)
- `signal<T>(valorInicial)` — estado mutável
- `computed(() => ...)` — estado derivado
- `effect(() => { ... })` — efeitos colaterais
- `input<T>()`, `output<T>()` — em componentes filhos

### Data flow
- Componentes pai passam dados via `[input]`
- Componentes filho emitem eventos via `(output)`
- Serviços com `providedIn: 'root'` compartilham estado global via signals

### HTML/CSS
- **NUNCA** usar template ou styles inline — sempre arquivos separados
- Usar `templateUrl` e `styleUrl` no decorator
- `:host { display: contents; }` em todos os componentes de página
- Classes globais disponíveis: `.btn`, `.btn-primary`, `.btn-danger`, `.btn-sm`, `.card`, `.badge`, `.page`, `.text-muted`, `.text-danger`

### Temas
- Fundo: `#0f172a` (body), `#1e293b` (cards/surfaces)
- Borda: `#334155`
- Texto primário: `#e2e8f0` / `#f1f5f9`
- Texto secundário: `#94a3b8` / `#64748b`
- Azul de destaque: `#3b82f6` / `#60a5fa`
- Input bg: `#0f172a`, border: `#475569`, focus: `#3b82f6`
- Usar `rgba(59,130,246,0.15)` para backgrounds de destaque (ex: stat icons)
- Ícones: Google Material Symbols (`<span class="material-symbols-outlined">nome</span>`)

### Sub-rotas (ex: configuracoes/categorias)
- `{ path: 'pai', redirectTo: 'pai/filho', pathMatch: 'full' }`
- `{ path: 'pai/filho', component: FilhoPage }`
- Sidebar com classe `.nav-item.sub` e `padding-left: 2.5rem`

## API Service (centralização de requisições)

Toda chamada à API deve usar o `ApiService` — **NUNCA** `fetch` ou `HttpClient` diretamente.

### Definição no enum `ApiEndpoint`

```typescript
export enum ApiEndpoint {
  AuthLogin = 'POST /api/auth/login',
  StockGetAll = 'GET /api/stock',
  StockGetById = 'GET /api/stock/:id',
  StockCreate = 'POST /api/stock',
  CategoriesDelete = 'DELETE /api/categories/:name',
  NotificationsUpdateEvent = 'PATCH /api/notifications/events/:eventKey',
  // ...
}
```

### Uso no componente

```typescript
import { ApiService } from '../../services/api.service';
import { ApiEndpoint } from '../../services/api-types';

@Component({ ... })
export class MeuComponente {
  private readonly api = inject(ApiService);

  async carregar() {
    // GET sem parâmetros
    const lista = await this.api.request<Item[]>(ApiEndpoint.StockGetAll);
    if (lista.success) this.items = lista.data;

    // GET com parâmetro de rota
    const item = await this.api.request<Item>(ApiEndpoint.StockGetById, {
      params: { id: 5 },
    });

    // POST com body
    const criado = await this.api.request<Item>(ApiEndpoint.StockCreate, {
      body: { name: 'Novo', quantity: 10 },
    });

    // DELETE
    await this.api.request(ApiEndpoint.StockDelete, {
      params: { id: 3 },
    });
  }
}
```

### Comportamento do `ApiService`
- `credentials: 'include'` — envia cookie `auth_token` automaticamente
- `Content-Type: application/json`
- Retorno tipado: `ApiResult<T>` = `{ success: true, data: T }` | `{ success: false, error: { message, status? } }`
- `401` → redireciona para `/login` automaticamente
- `204 No Content` → retorna `success: true` com `data: undefined`
- Erro de rede → `{ success: false, error: { message: 'Erro de conexão...' } }`
- AbortSignal suportado via `options.signal`

### Adicionar novo endpoint

1. Criar no backend (Controller → Service → Repository → QueryProvider)
2. Adicionar ao enum `ApiEndpoint` em `services/api-types.ts`
3. Usar no frontend via `apiService.request(ApiEndpoint.NovoEndpoint, ...)`

## Criar um subcomponente

Exemplo: `modules/estoque/estoque-list.ts/.html/.css`

```typescript
@Component({
  selector: 'app-stock-list',
  imports: [DatePipe],
  templateUrl: './stock-list.html',
  styleUrl: './stock-list.css'
})
export class StockList {
  readonly items = input<Item[]>([]);       // recebe do pai
  readonly add = output();                   // emite evento pro pai
  readonly edit = output<Item>();
  readonly delete = output<number>();
}
```

Template do pai usa binding: `[items]="data" (add)="handler()" (edit)="handler($event)"`

## PWA
- Service worker registrado em `app.config.ts` via `provideServiceWorker`
- Habilitado apenas em produção (`!isDevMode()`)
- `ngsw-config.json` com prefetch do app shell
- Manifest em `public/manifest.webmanifest`
- Ícones SVG em `public/icons/`

## Login
- Sidebar oculta automaticamente na rota `/login` (lógica em `app.ts`)
- Mock: `admin@ong.org` / `admin`

## Docker
- `npm ci` no Dockerfile (exige lockfile sincronizado)
- Após `npm install`, rebuildar com `docker compose up --build -V -d`
- Flag `-V` recria volumes anônimos (necessário para node_modules novo)