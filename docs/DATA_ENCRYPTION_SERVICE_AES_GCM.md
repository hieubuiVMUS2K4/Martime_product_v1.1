# DataEncryptionService dung AES-GCM

## 1. Muc dich tai lieu

Tai lieu nay mo ta dich vu `DataEncryptionService` dang duoc trien khai trong Shore backend de bao ve du lieu nhay cam khi luu tru. Muc tieu cua tai lieu la:

- giai thich `DataEncryptionService` la gi trong kien truc he thong;
- mo ta cach trien khai thuc te trong du an;
- giai thich tac dung bao mat va gia tri van hanh;
- trinh bay co so toan hoc va thuat toan cua `AES-GCM` o muc co the dua vao bao cao nghien cuu khoa hoc.

Tai lieu nay bo sung cho [docs/SHORE_DATA_PROTECTION_KEY_ROTATION.md](SHORE_DATA_PROTECTION_KEY_ROTATION.md), nhung tap trung vao ban than dich vu ma hoa thay vi quy trinh rotate khoa.

## 2. DataEncryptionService la gi

`DataEncryptionService` la mot dich vu ma hoa doi xung trong Shore backend, duoc dang ky DI tai [shore_product/backend/Program.cs](../shore_product/backend/Program.cs) va hien thuc tai [shore_product/backend/Security/DataEncryptionService.cs](../shore_product/backend/Security/DataEncryptionService.cs).

Ve ban chat, day la lop `application security service` nam tren tang ung dung, co nhiem vu:

- ma hoa secret truoc khi ghi vao CSDL hoac file system;
- giai ma khi can su dung secret hoac noi dung file do;
- ho tro nhieu version khoa de rotate khoa an toan;
- duy tri backward compatibility voi du lieu cu chua ma hoa hoac du lieu dang dung envelope cu.

No khong thay the co che `HMAC` cua signed sync. Trong du an nay:

- `HMAC` bao ve tinh xac thuc va toan ven cua request dong bo tren duong truyen;
- `AES-GCM` bao ve tinh bi mat va toan ven cua du lieu khi luu tru tai Shore.

Noi cach khac, `HMAC` la bao ve `data in transit`, con `DataEncryptionService` la bao ve `data at rest`.

## 3. Bai toan bao mat ma dich vu nay giai quyet

Neu khong co lop ma hoa at-rest, he thong gap cac rui ro sau:

- khoa dong bo giua Edge va Shore co the bi lo neu CSDL bi dump;
- file nhay cam co the bi doc truc tiep tu dia neu server hoac backup bi truy cap trai phep;
- khi rotate khoa, he thong de mat kha nang doc du lieu cu neu khong co co che version hoa envelope;
- viec phan tach quyen truy cap API khong du de bao ve du lieu neu attacker da co duoc ban sao raw storage.

`DataEncryptionService` duoc them vao de giam cac rui ro do bang cach ma hoa secret va file nhay cam truoc khi luu, dong thoi gan them thong tin version khoa trong envelope de co the giai ma dung khoa ve sau.

## 4. Vi tri cua dich vu trong kien truc hien tai

Trong du an hien tai, dich vu duoc dung o hai nhom chinh:

### 4.1. Bao ve sync signing key

- duoc inject vao [shore_product/backend/Security/SyncRequestVerificationMiddleware.cs](../shore_product/backend/Security/SyncRequestVerificationMiddleware.cs);
- duoc inject vao [shore_product/backend/Controllers/SyncDashboardController.cs](../shore_product/backend/Controllers/SyncDashboardController.cs);
- gia tri `SigningKey` va `PreviousSigningKey` cua node sync duoc ma hoa truoc khi ghi, va giai ma khi can xac minh request HMAC.

### 4.2. Bao ve file nghiep vu nhay cam

- duoc inject vao [shore_product/backend/Services/Sync/SyncFileStorageService.cs](../shore_product/backend/Services/Sync/SyncFileStorageService.cs);
- cac file duoi cac prefix sau duoc ma hoa khi ghi xuong dia:
  - `/uploads/crew/avatars/`
  - `/uploads/crew/certificates/`
  - `/uploads/crew/documents/`
- route public static cho `/uploads` da duoc thay bang controller co authorize tai [shore_product/backend/Controllers/ProtectedUploadsController.cs](../shore_product/backend/Controllers/ProtectedUploadsController.cs).

## 5. Cach trien khai trong code

### 5.1. Interface nghiep vu

Interface `IDataEncryptionService` cung cap 7 thao tac:

- `IsConfigured`: kiem tra da co khoa hay chua;
- `Encrypt(string?)`: ma hoa chuoi secret;
- `Decrypt(string?)`: giai ma chuoi secret;
- `IsEncrypted(string?)`: nhan dien envelope chuoi;
- `EncryptBytes(byte[])`: ma hoa du lieu nhi phan;
- `DecryptBytes(byte[])`: giai ma du lieu nhi phan;
- `IsEncryptedPayload(byte[]?)`: nhan dien envelope file nhi phan.

### 5.2. Cau hinh khoa

He thong doc khoa tu phan `DataProtection` trong config:

```json
{
  "DataProtection": {
    "EncryptionKey": "legacy-or-fallback-key",
    "CurrentKeyVersion": "v1",
    "EncryptionKeys": {
      "v1": "key-material-for-v1",
      "v2": "key-material-for-v2"
    }
  }
}
```

Y nghia tung truong:

- `EncryptionKey`: khoa fallback cho du lieu string cu dang dung envelope `enc:v1:`;
- `CurrentKeyVersion`: version dang duoc dung de ghi du lieu moi;
- `EncryptionKeys`: tap cac khoa duoc phep dung de doc cac envelope versioned.

### 5.3. Chuan hoa key material

Code ho tro 2 cach nhap khoa:

- base64 AES key hop le co do dai `16`, `24` hoac `32` byte;
- raw secret co do dai it nhat `32` byte UTF-8.

Neu dau vao la raw secret, he thong khong dung truc tiep chuoi do lam khoa AES, ma hash bang `SHA-256` de suy ra khoa `256-bit`:

$$
K = \operatorname{SHA256}(S)
$$

Trong do:

- $S$ la raw secret;
- $K$ la khoa AES-256 thu duoc sau khi hash.

### 5.4. Envelope cho string

Du lieu string moi duoc luu theo dang:

```text
enc:v2:<keyVersion>:Base64(nonce || tag || ciphertext)
```

Trong do:

- `enc:v2:` la prefix nhan dien dinh dang moi;
- `<keyVersion>` la version khoa, vi du `v1`, `v2`;
- `nonce` dai `12` byte;
- `tag` dai `16` byte;
- `ciphertext` la ban ma.

Du lieu string cu van co the ton tai theo dang:

```text
enc:v1:Base64(nonce || tag || ciphertext)
```

### 5.5. Envelope cho file nhi phan

Du lieu file moi duoc ma hoa theo envelope nhi phan co prefix ASCII `encf:v2:`. Cau truc logic la:

```text
binaryPrefix || versionLength || versionBytes || nonce || tag || ciphertext
```

Trong do:

- `binaryPrefix = "encf:v2:"`;
- `versionLength` la 1 byte;
- `versionBytes` la xau version khoa duoi dang UTF-8;
- `nonce` dai `12` byte;
- `tag` dai `16` byte;
- `ciphertext` la noi dung file da ma hoa.

Thiet ke nay cho phep file moi tu mang theo thong tin khoa can dung, khong can dua vao metadata ngoai he thong.

### 5.6. Additional Authenticated Data

Code hien tai dung AAD co gia tri hang:

```text
ProductApi.DataEncryption.v1
```

AAD khong bi ma hoa nhung duoc dua vao qua trinh xac thuc. Neu attacker sua AAD hoac su dung AAD khac, tag se khong con hop le va qua trinh giai ma se that bai.

## 6. AES-GCM la gi

`AES-GCM` la viet tat cua `Advanced Encryption Standard - Galois/Counter Mode`. Day la mot `AEAD` mode, tuc la `Authenticated Encryption with Associated Data`.

No ket hop hai thanh phan:

- ma hoa bang `counter mode` de tao `ciphertext`;
- xac thuc bang `GHASH` tren truong `GF(2^{128})` de tao `authentication tag`.

Vi vay, AES-GCM dat dong thoi 2 muc tieu:

- bi mat: attacker khong the doc duoc plaintext neu khong co khoa;
- toan ven va xac thuc: attacker khong the sua payload ma van vuot qua kiem tra tag.

## 7. Co so toan hoc cua AES-GCM

### 7.1. Ky hieu

- $K$: khoa doi xung AES;
- $N$: nonce hoac IV;
- $P$: plaintext;
- $C$: ciphertext;
- $A$: additional authenticated data;
- $T$: authentication tag;
- $E_K(\cdot)$: ham ma hoa AES voi khoa $K$;
- $H = E_K(0^{128})$: subkey dung cho GHASH.

### 7.2. Phan ma hoa counter mode

Voi moi block plaintext $P_i$, he thong sinh keystream block bang AES tren bo dem `counter`:

$$
S_i = E_K(J_i)
$$

Sau do tao ciphertext:

$$
C_i = P_i \oplus S_i
$$

Trong do:

- $J_i$ la counter block thu $i$;
- $\oplus$ la phep XOR.

Neu su dung nonce `96-bit`, counter khoi tao duoc tao theo chuan GCM bang cach ghep nonce voi `0^{31} || 1`:

$$
J_0 = N \parallel 0^{31} \parallel 1
$$

### 7.3. Phan xac thuc GHASH

GCM dung ham `GHASH` tren truong Galois `GF(2^{128})`. Neu chia du lieu thanh cac block `128-bit` la $X_1, X_2, ..., X_m$ thi:

$$
Y_0 = 0^{128}
$$

$$
Y_i = (Y_{i-1} \oplus X_i) \cdot H \quad \text{trong } GF(2^{128})
$$

Khi do:

$$
\operatorname{GHASH}_H(X) = Y_m
$$

Trong GCM, day $X$ bao gom:

- cac block cua `AAD`;
- cac block cua `ciphertext`;
- block do dai cua `AAD` va `ciphertext`.

### 7.4. Cong thuc tag xac thuc

Tag cuoi cung duoc tinh bang:

$$
T = E_K(J_0) \oplus \operatorname{GHASH}_H(A, C)
$$

Neu bat ky bit nao trong $A$ hoac $C$ bi sua doi, tag tinh lai se khac, va giai ma se that bai.

### 7.5. Tinh chat an toan quan trong

AES-GCM an toan khi va chi khi nonce khong bi tai su dung voi cung mot khoa. Neu hai thong diep khac nhau dung cung mot cap $(K, N)$, attacker co the khai thac quan he giua hai ciphertext:

$$
C_1 \oplus C_2 = P_1 \oplus P_2
$$

Day la ly do code hien tai luon sinh nonce ngau nhien moi bang `RandomNumberGenerator.GetBytes(12)`.

## 8. Thuat toan trien khai trong du an

### 8.1. Thuat toan ma hoa string

```text
Input: plaintext, currentKeyVersion, EncryptionKeys
Output: enc:v2:<keyVersion>:<base64Payload>

1. Neu plaintext rong, tra ve nguyen trang.
2. Neu plaintext da co prefix enc:v1: hoac enc:v2:, khong ma hoa lai.
3. Lay khoa hien hanh theo CurrentKeyVersion.
4. Sinh nonce 12 byte ngau nhien.
5. Tinh AES-GCM(K, nonce, plaintext, AAD) -> (ciphertext, tag).
6. Ghep payload = nonce || tag || ciphertext.
7. Base64 payload.
8. Tra ve chuoi enc:v2:<keyVersion>:<base64Payload>.
```

### 8.2. Thuat toan giai ma string

```text
Input: protectedValue
Output: plaintext

1. Neu gia tri rong, tra ve nguyen trang.
2. Neu khong co prefix enc:v1: hoac enc:v2:, xem nhu plaintext legacy va tra ve nguyen trang.
3. Neu la enc:v1:, dung EncryptionKey legacy de giai ma.
4. Neu la enc:v2:, tach keyVersion va payload, tim khoa tu EncryptionKeys[keyVersion].
5. Tach nonce, tag, ciphertext tu payload.
6. Giai ma bang AES-GCM(K, nonce, ciphertext, tag, AAD).
7. Neu tag sai, nem exception.
8. Tra ve plaintext.
```

### 8.3. Thuat toan ma hoa file

```text
Input: fileBytes, currentKeyVersion
Output: protectedBinaryPayload

1. Neu payload da co prefix encf:v2:, bo qua de tranh ma hoa hai lan.
2. Sinh nonce 12 byte ngau nhien.
3. Tinh AES-GCM(K, nonce, fileBytes, AAD) -> (ciphertext, tag).
4. Ma hoa keyVersion sang UTF-8, lay versionLength = 1 byte.
5. Ghep binaryPrefix || versionLength || versionBytes || nonce || tag || ciphertext.
6. Ghi payload do xuong dia.
```

### 8.4. Thuat toan giai ma file

```text
Input: protectedPayload
Output: plaintextFileBytes

1. Neu khong co prefix encf:v2:, xem nhu file plaintext legacy va tra ve nguyen trang.
2. Doc versionLength va versionBytes.
3. Tim khoa phu hop voi version.
4. Tach nonce, tag, ciphertext.
5. Giai ma bang AES-GCM.
6. Neu tag khong hop le, dung qua trinh va bao loi.
7. Tra ve file bytes plaintext.
```

## 9. Tai sao chon AES-GCM thay vi CBC hoac chi hash

### 9.1. So voi AES-CBC

`AES-CBC` chi giai quyet bai toan bi mat. Neu muon co them toan ven, can ghep them `HMAC`, dan toi mo hinh `Encrypt-then-MAC` phuc tap hon, de sai hon va kho quan ly hon.

`AES-GCM` cho phep:

- ma hoa va xac thuc trong cung mot primitive;
- hieu nang tot tren phan cung hien dai;
- duoc ho tro native trong .NET qua `AesGcm`;
- de mo rong voi `AAD` va versioned envelope.

### 9.2. So voi chi hash

Ham hash nhu `SHA-256` chi cho phep kiem tra toan ven, nhung khong che giau du lieu. Neu chi hash secret roi luu, he thong se khong the phuc hoi secret de tiep tuc dung cho HMAC sync hoac giai ma file. Bai toan nay can `reversible encryption`, vi vay phai dung ma hoa doi xung thay vi hash mot chieu.

### 9.3. So voi chi dung HMAC

`HMAC` xac minh toan ven va xac thuc, nhung van de lo nguyen van plaintext neu attacker co quyen doc storage. Vi vay HMAC khong thay the duoc ma hoa at-rest.

## 10. Tac dung thuc te trong du an

### 10.1. Doi voi sync node key

Truoc khi co dich vu nay, khoa dong bo co nguy co xuat hien duoi dang plaintext trong CSDL. Sau khi trien khai:

- khoa duoc ma hoa truoc khi luu;
- middleware van giai ma duoc dung version khoa de tiep tuc verify HMAC;
- viec rotate khoa khong lam vo signed sync vi du lieu cu van doc duoc.

### 10.2. Doi voi file nhay cam

File avatar, certificate va crew document khong con la file plaintext nam public tren dia. Hien nay:

- file duoc ma hoa khi ghi;
- route lay file phai qua controller co authorize;
- sync service doc file qua storage service de lam viec tren plaintext logic thay vi ciphertext.

### 10.3. Doi voi backup va su co lo du lieu

Neu CSDL hoac thu muc upload bi sao chep trai phep, attacker chi nhan duoc:

- ciphertext;
- nonce;
- tag;
- thong tin version khoa.

Neu khong co key material dung, attacker khong the phuc hoi plaintext co nghia.

## 11. Gioi han va diem can luu y

### 11.1. Nonce reuse la toi ky

Khong duoc tai su dung cung mot nonce voi cung mot khoa. Day la gia thiet quan trong nhat cua GCM.

### 11.2. Bao mat khoa van la cot loi

Neu `DataProtection:EncryptionKeys` bi lo, attacker van co the giai ma du lieu. Vi vay can luu khoa trong secret manager, khong hard-code production key trong repo.

### 11.3. Khong thay the phan quyen

Ma hoa at-rest khong thay the `authorization`. He thong van can:

- auth;
- policy `InternalAccess`;
- audit log;
- secure sync HMAC.

### 11.4. Khong tu dong re-encrypt du lieu cu

Versioned key rotation cho phep doc du lieu cu, nhung khong co nghia du lieu cu tu dong duoc ghi lai bang khoa moi. Neu muon chuan hoa toan bo du lieu sang khoa moi, can co batch re-encryption rieng.

## 12. Y nghia hoc thuat va diem dong gop co the trinh bay

Tu goc do nghien cuu, `DataEncryptionService` co the duoc trinh bay nhu mot thanh phan bo sung cho kien truc secure sync, nham mo rong bao mat tu `communication security` sang `storage security`.

Diem dong gop co the neu trong bao cao:

- de xuat mot co che `versioned at-rest encryption` cho he thong Edge-Shore;
- ket hop `HMAC` cho signed sync voi `AES-GCM` cho bao ve secret va file tai Shore;
- dam bao backward compatibility voi du lieu legacy thong qua envelope version hoa;
- dam bao rotate khoa ma khong pha vo dong bo dang van hanh;
- dong nhat co che giai ma thong qua storage abstraction de khong pha logic checksum va file transfer.

## 13. Mau dien giai de dua vao bao cao nghien cuu khoa hoc

Co the su dung hoac chinh sua doan sau:

> De tang muc bao ve du lieu nhay cam tai Shore, he thong bo sung mot dich vu ma hoa doi xung `DataEncryptionService` dua tren `AES-GCM`. Khac voi co che signed sync dung `HMAC` chi bao ve tinh xac thuc va toan ven cua thong diep tren duong truyen, `DataEncryptionService` bao ve secret va file nghiep vu khi luu tru trong co so du lieu va file system. Ve ky thuat, dich vu su dung nonce `96-bit`, authentication tag `128-bit`, AAD co dinh de rang buoc ngu canh ung dung, va envelope version hoa de ho tro rotate khoa an toan. Voi thiet ke nay, du lieu moi duoc ma hoa bang khoa hien hanh, trong khi du lieu cu van co the giai ma bang khoa phu hop voi version da ghi trong envelope. Cach tiep can nay giup mo rong secure sync thanh mot mo hinh bao mat bao phu ca `data in transit` lan `data at rest`, dong thoi duy tri kha nang van hanh va tuong thich nguoc trong moi truong phan tan Edge-Shore.

## 14. Mau tom tat ngan cho slide hoac abstract

> `DataEncryptionService` la lop ma hoa at-rest cua Shore backend, su dung `AES-GCM` de bao ve sync secret va file nhay cam. Dich vu nay cung cap tinh bi mat, toan ven, ho tro AAD, va key rotation theo version, qua do bo sung cho signed sync HMAC va nang muc an toan cua he thong tu tang truyen thong len tang luu tru.

## 15. Ket luan

`DataEncryptionService` dung `AES-GCM` la mot thanh phan bao mat co y nghia thuc te va hoc thuat trong du an nay. Ve thuc te, no giam rui ro lo secret va file neu storage bi truy cap trai phep. Ve ky thuat, no dua vao mot AEAD mode chuan, hieu nang tot, co co so toan hoc ro rang, va de trien khai an toan trong .NET. Ve nghien cuu, no giup hoan thien lap luan rang he thong khong chi bao ve kenh dong bo ma con bao ve du lieu nhay cam trong suot vong doi luu tru.