# SysPost — Guia Rápido

## Usuários e Senhas

| Papel | Email | Senha |
|-------|-------|-------|
| SuperAdmin | super@sis.com | Super@123 |
| Admin | admin@sis.com | Admin@123 |
| Usuário | usuario@sis.com | Usuario@123 |

---

## Problemas Comuns

### 1. `SqlException: Nome de objeto '...' inválido`
**Causa:** Migration foi criada mas não aplicada ao banco.  
**Solução:**
```
dotnet ef database update
```
Ou reinicie o app — o `Program.cs` aplica migrations automaticamente na inicialização.

### 2. `NullReferenceException` ao criar/editar post
**Causa:** Usuário logado no cookie não existe mais no banco (ex: após drop/recreate).  
**Solução:** Faça logout e login novamente.

### 3. Porta em uso ao rodar o app
**Solução:** Feche o processo antigo:
```powershell
Get-Process -Name SysPost -ErrorAction SilentlyContinue | Stop-Process -Force
```

### 4. Erro de FK ao excluir post com comentários
**Causa:** Tentativa de deletar post sem remover comentários primeiro.  
**Solução:** Já corrigido no código — `Include(p => p.Comentarios)` + `RemoveRange`.

---

## Migrations

### Criar uma migration nova
```
dotnet ef migrations add NomeDaMigration
```

### Deletar a última migration (não aplicada)
```
dotnet ef migrations remove
```

---

## Banco de Dados

### Recriar o banco do zero (drop + update)
```
dotnet ef database drop --force
dotnet ef database update
```

> ⚠️ Isso apaga **todos os dados**. O seed rodará na próxima inicialização do app.

### Banco já existe, só aplicar pendências
```
dotnet ef database update
```

Ou apenas inicie o app — `db.Database.Migrate()` no `Program.cs` aplica automaticamente.

### Migrations para outra connection string

Crie um `appsettings.Development.json` local com a connection desejada:

```json
{
  "ConnectionStrings": {
    "ConexaoPadrao": "Server=OUTRO_SERVER;Database=fsapp;..."
  }
}
```

Depois rode a migration normalmente:
```
dotnet ef database update
```

O EF Core usa a connection string do `appsettings.json` / `appsettings.Development.json` automaticamente — **não precisa alterar o `Program.cs`**.
