#!/bin/bash
set -e

BASE="http://localhost:5045"
COOKIE_JAR=$(mktemp)
PASS=0
FAIL=0
RESULTS=()
SUMMARY=""

cleanup() { rm -f "$COOKIE_JAR"; }
trap cleanup EXIT

test() {
  local method="$1" url="$2" desc="$3" expect="$4"
  local data="$5"  # optional JSON body
  
  local args=(-s -b "$COOKIE_JAR" -c "$COOKIE_JAR" -w "\n%{http_code}" -X "$method")
  if [ -n "$data" ]; then
    args+=(-H "Content-Type: application/json" -d "$data")
  fi
  
  local output code body
  output=$(curl "${args[@]}" "$url" 2>/dev/null)
  code=$(echo "$output" | tail -1)
  body=$(echo "$output" | sed '$d')
  
  local status="✅"
  if [ "$code" = "$expect" ]; then
    PASS=$((PASS+1))
    RESULTS+=("PASS:$desc")
  else
    status="❌"
    FAIL=$((FAIL+1))
    RESULTS+=("FAIL:$desc")
  fi
  printf "  %s %s → %s (expected %s)\n" "$status" "$desc" "$code" "$expect"
  if [ "$code" != "$expect" ] && [ -n "$body" ]; then
    echo "     $(echo "$body" | head -c 150)"
  fi
}

echo "============================================"
echo "  INTEGRATION TESTS - ONG Estoque API"
echo "============================================"
echo ""

# ─── 1. AUTH ───────────────────────────────────
echo "[1] Auth"

test POST "$BASE/api/auth/login" "Login válido" "200" \
  '{"email":"admin@ong.org","password":"admin"}'

test POST "$BASE/api/auth/login" "Login senha errada" "401" \
  '{"email":"admin@ong.org","password":"wrong"}'

test POST "$BASE/api/auth/login" "Login email inexistente" "401" \
  '{"email":"noone@ong.org","password":"admin"}'

test POST "$BASE/api/auth/login" "Login body vazio" "400" \
  '{}'

test GET "$BASE/api/auth/me" "Verificar sessão ativa" "200"

test POST "$BASE/api/auth/logout" "Logout" "200"

test GET "$BASE/api/auth/me" "Sessão após logout" "401"

# re-login for next sections
curl -s -b "$COOKIE_JAR" -c "$COOKIE_JAR" -X POST "$BASE/api/auth/login" \
  -H "Content-Type: application/json" -d '{"email":"admin@ong.org","password":"admin"}' > /dev/null

echo ""

# ─── 2. INBOUND PROCESSES ──────────────────────
echo "[2] Inbound Processes"

test GET "$BASE/api/inbound?page=1&pageSize=5" "Listar paginado" "200"
test GET "$BASE/api/inbound?page=1&pageSize=0" "PageSize=0 (default 20)" "200"
test GET "$BASE/api/inbound?page=0&pageSize=10" "Page=0 (default 1)" "200"
test GET "$BASE/api/inbound?page=99&pageSize=10" "Página inexistente" "200"
test GET "$BASE/api/inbound/all" "Listar todos" "200"
test GET "$BASE/api/inbound/1" "Buscar por ID" "200"
test GET "$BASE/api/inbound/9999" "ID inexistente" "404"

# Create entry process
test POST "$BASE/api/inbound" "Criar entrada" "201" \
  '{"name":"Teste Integração","description":"Teste automático","startDate":"2026-08-01","endDate":"2026-08-31","type":"entry"}'

# Get created ID
NEW_ID=$(curl -s -b "$COOKIE_JAR" "$BASE/api/inbound?page=1&pageSize=50" | python3 -c "
import sys, json
data = json.load(sys.stdin)['data']
for p in data:
    if p['name'] == 'Teste Integração':
        print(p['id'])
        break
" 2>/dev/null)

if [ -n "$NEW_ID" ]; then
  test PATCH "$BASE/api/inbound/$NEW_ID/pause" "Pausar" "200"
  test PATCH "$BASE/api/inbound/$NEW_ID/resume" "Retomar" "200"
  test PATCH "$BASE/api/inbound/$NEW_ID/finish" "Finalizar" "200"
  test PATCH "$BASE/api/inbound/$NEW_ID/finish" "Finalizar já finalizado" "200"
  
  # Create another for cancel test
  C_ID=$(curl -s -b "$COOKIE_JAR" -X POST "$BASE/api/inbound" \
    -H "Content-Type: application/json" \
    -d '{"name":"Cancelamento Test","description":"","startDate":"2026-09-01","endDate":"2026-09-15","type":"entry"}' | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])" 2>/dev/null)
  test PATCH "$BASE/api/inbound/$C_ID/cancel" "Cancelar" "200"
fi

# Create exit process
test POST "$BASE/api/inbound" "Criar saída" "201" \
  '{"name":"Teste Saída","description":"","startDate":"2026-08-10","endDate":"2026-08-20","type":"exit"}'

echo ""

# ─── 3. INBOUND ITEMS ──────────────────────────
echo "[3] Inbound Items"

test POST "$BASE/api/inbound/1/items" "Adicionar item c/ validade" "201" \
  '{"productTypeId":9,"itemId":null,"name":"Arroz Teste","quantity":1,"unit":"unidades","expiryDate":"2027-12-31"}'

test POST "$BASE/api/inbound/1/items" "Adicionar item s/ validade" "201" \
  '{"productTypeId":10,"itemId":null,"name":"Feijão Teste","quantity":1,"unit":"unidades","expiryDate":null}'

test GET "$BASE/api/inbound/1/items?page=1&pageSize=50" "Listar itens" "200"
test GET "$BASE/api/inbound/9999/items" "Processo inexistente" "200"

# Delete an item
ITEM_ID=$(curl -s -b "$COOKIE_JAR" "$BASE/api/inbound/1/items?page=1&pageSize=50" | python3 -c "
import sys, json
d = json.load(sys.stdin)
if d['data']:
    print(d['data'][0]['id'])
" 2>/dev/null)

if [ -n "$ITEM_ID" ]; then
  test DELETE "$BASE/api/inbound/1/items/$ITEM_ID" "Excluir item" "204"
fi

# Try to delete from completed process
ITEM_ID2=$(curl -s -b "$COOKIE_JAR" "$BASE/api/inbound/2/items?page=1&pageSize=50" | python3 -c "
import sys, json
d = json.load(sys.stdin)
if d['data']:
    print(d['data'][0]['id'])
" 2>/dev/null)
if [ -n "$ITEM_ID2" ]; then
  test DELETE "$BASE/api/inbound/2/items/$ITEM_ID2" "Excluir de finalizado (falha)" "400"
fi

echo ""

# ─── 4. MOVEMENTS ──────────────────────────────
echo "[4] Movements"

test GET "$BASE/api/movements?page=1&pageSize=5" "Listar paginado" "200"
test GET "$BASE/api/movements?page=1&pageSize=100" "PageSize grande" "200"
test GET "$BASE/api/movements?page=3&pageSize=5" "Página 3 (vazia)" "200"

echo ""

# ─── 5. STOCK ───────────────────────────────────
echo "[5] Stock"

test GET "$BASE/api/stock?page=1&pageSize=5" "Listar paginado" "200"
test GET "$BASE/api/stock/all" "Listar todos" "200"
test GET "$BASE/api/stock/1" "Buscar por ID" "200"
test GET "$BASE/api/stock/9999" "ID inexistente" "404"

echo ""

# ─── 6. PRODUCT TYPES ───────────────────────────
echo "[6] Product Types"

test GET "$BASE/api/product-types" "Listar tipos" "200"

echo ""

# ─── 7. UNAUTHORIZED ────────────────────────────
echo "[7] Unauthorized"

rm -f "$COOKIE_JAR"
touch "$COOKIE_JAR"

test GET "$BASE/api/inbound?page=1&pageSize=3" "Inbound sem auth" "401"
test GET "$BASE/api/movements?page=1&pageSize=3" "Movements sem auth" "401"
test GET "$BASE/api/stock?page=1&pageSize=3" "Stock sem auth" "401"

echo ""
echo "============================================"
echo "  RESULTS"
echo "============================================"
echo "  ✅ Passed: $PASS"
echo "  ❌ Failed: $FAIL"
echo ""

if [ "$FAIL" -gt 0 ]; then
  echo "  Failed tests:"
  for r in "${RESULTS[@]}"; do
    if [[ "$r" == FAIL:* ]]; then
      echo "    - ${r#FAIL:}"
    fi
  done
  echo ""
  exit 1
else
  echo "  ✅ All $PASS tests passed!"
  echo ""
fi
