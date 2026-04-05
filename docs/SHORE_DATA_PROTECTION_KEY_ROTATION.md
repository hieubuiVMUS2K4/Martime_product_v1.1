## Shore DataProtection Key Rotation

### Muc tieu

Tai lieu nay mo ta cau hinh toi thieu de Shore ma hoa secret va file nhay cam bang AES-GCM, dong thoi cach rotate khoa an toan ma khong lam mat kha nang doc du lieu cu.

### Cau hinh dev mac dinh

Shore backend da duoc cau hinh san trong [shore_product/backend/appsettings.Development.json](shore_product/backend/appsettings.Development.json) voi:

- `DataProtection:EncryptionKey = LocalDevDataProtectionKey_Shore_2026_AtLeast_32_Chars`
- `DataProtection:CurrentKeyVersion = v1`
- `DataProtection:EncryptionKeys:v1 = LocalDevDataProtectionKey_Shore_2026_AtLeast_32_Chars`

Cau hinh nay chi de phuc vu local development. Khong duoc dua nguyen gia tri nay len moi truong public.

### Cau hinh production toi thieu

- Dat `DataProtection:CurrentKeyVersion` thanh version dang ghi moi, vi du `v2`.
- Dat `DataProtection:EncryptionKeys:v1`, `DataProtection:EncryptionKeys:v2`, ... cho moi khoa con can doc.
- Giu `DataProtection:EncryptionKey` trong giai doan chuyen doi neu van con du lieu string cu dang dung envelope `enc:v1:`.

Vi du:

```json
{
  "DataProtection": {
    "EncryptionKey": "legacy-fallback-secret-for-enc-v1-values",
    "CurrentKeyVersion": "v2",
    "EncryptionKeys": {
      "v1": "old-32-byte-secret-or-base64-key",
      "v2": "new-32-byte-secret-or-base64-key"
    }
  }
}
```

### Quy trinh rotate khoa an toan

1. Tao khoa moi `vN+1` va them vao `DataProtection:EncryptionKeys` tren tat ca instance Shore.
2. Chua doi `CurrentKeyVersion`; rollout truoc de tat ca instance deu doc duoc ca khoa cu va moi.
3. Doi `CurrentKeyVersion` sang `vN+1`.
4. Tu thoi diem nay, du lieu moi se duoc ghi bang khoa moi, du lieu cu van doc bang khoa cu.
5. Thuc hien backfill neu muon re-encrypt du lieu cu sang khoa moi.
6. Chi xoa khoa cu sau khi da xac nhan khong con payload nao phu thuoc vao no.

### Ghi chu migration

- String secret cu dang duoc bao ve bang `enc:v1:` van can `DataProtection:EncryptionKey` de doc trong giai doan qua do.
- File duoc bao ve bang envelope nhi phan `encf:v2:` co metadata version, vi vay co the song song nhieu khoa ma khong can fallback legacy.
- Cac file duoi `/uploads/crew/avatars`, `/uploads/crew/certificates`, `/uploads/crew/documents` hien da duoc ma hoa trong luc ghi file.