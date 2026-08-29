# SalesGO
# Mobile Sales Force Automation

## Profil Kapabilitas Produk

---

**Pernyataan posisi**

SalesGO adalah aplikasi mobile untuk sales lapangan distributor. Sales dapat mengambil order pelanggan dan membuktikan kunjungan di lokasi outlet—termasuk saat tidak ada jaringan—kemudian mengirim hasil kerja ke sistem distributor.

Dokumen ini menjelaskan kapabilitas operasional produk untuk Owner, Direktur, General Manager, Sales Manager, dan Operational Manager. Tujuannya membantu menjawab: *Apakah SalesGO relevan untuk kebutuhan operasional distributor kami?*

> **Catatan ruang lingkup:** SalesGO mendukung eksekusi sales lapangan (order dan kunjungan). Produk ini tidak mencakup penagihan piutang, pengiriman barang, operasi gudang, atau mesin promosi.

---

# 1. Ringkasan Eksekutif

| Aspek | Penjelasan |
|--------|------------|
| **Apa itu SalesGO** | Aplikasi Sales Force Automation (SFA) mobile untuk sales distributor di lapangan |
| **Pengguna utama** | Sales lapangan (salesman / sales representative) |
| **Penerima data di kantor** | Admin sales / back-office (bukan pengguna aplikasi; menerima order, kunjungan, dan lokasi pelanggan yang sudah disinkronkan) |
| **Cabang yang didukung** | Operasi terikat depot/cabang (saat ini Jogja atau Magelang) |

**Aktivitas operasional yang didukung**

- Membuat dan menyelesaikan order penjualan di lapangan
- Melakukan check-in / check-out kunjungan pelanggan berbasis GPS
- Melengkapi koordinat lokasi outlet yang belum memiliki pin GPS
- Mengunduh data master (pelanggan, produk, salesman) untuk kerja offline
- Mengunggah order, kunjungan, dan lokasi pelanggan ke sistem distributor
- Melihat ringkasan order harian di perangkat sales

**Nilai bisnis yang diberikan**

- Order dapat dicatat di lokasi pelanggan tanpa menunggu sales kembali ke kantor
- Kunjungan dapat dikaitkan dengan keberadaan fisik di dekat outlet
- Lokasi outlet yang belum terdokumentasi dapat dilengkapi dari lapangan
- Data order dan kunjungan masuk ke sistem pusat untuk diproses kantor

> **Untuk siapa relevan?** Organisasi distributor yang ingin mendisiplinkan kunjungan sales lapangan dan mempercepat masuknya order ke sistem operasional—tanpa mengganti modul piutang, pengiriman, atau gudang.

---

# 2. Tantangan Operasional Distributor

Tantangan berikut umum di operasi sales lapangan dan secara langsung didukung oleh kapabilitas SalesGO saat ini.

| Tantangan operasional | Dampak di lapangan | Dukungan SalesGO |
|------------------------|--------------------|------------------|
| Order masih dicatat manual (kertas / catatan terpisah) | Order baru diolah setelah sales kembali ke kantor; risiko salah tulis dan penundaan fulfillment | Order dibuat di perangkat, offline, lalu diunggah ke sistem |
| Kunjungan pelanggan sulit diverifikasi | Sulit memastikan sales benar-benar datang ke outlet | Check-in GPS hanya untuk pelanggan dalam radius 100 meter |
| Lokasi outlet tidak terdokumentasi dengan baik | Sulit menavigasi, memverifikasi kunjungan, dan membangun data lokasi | Registrasi GPS outlet (sekali, jika belum ada pin) dari lapangan |
| Aktivitas sales baru diketahui setelah kembali ke kantor | Supervisi bergantung laporan lisan atau rekap kertas | Jejak kunjungan dan order tersinkron ke sistem distributor |
| Order distributor memakai satuan besar–kecil, bonus, dan diskon berjenjang | Catatan lapangan sering tidak lengkap atau harus dihitung ulang di kantor | Input qty besar/kecil, bonus, dan hingga empat tingkat diskon per baris |

---

# 3. Kapabilitas Utama

## 3.1 Otomasi Pengambilan Order

| | |
|--|--|
| **Tujuan bisnis** | Mengubah permintaan pelanggan di lapangan menjadi order terstruktur yang siap diproses kantor |
| **Penggunaan operasional** | Sales membuat order offline, memilih pelanggan dan salesman, menambah baris produk (qty besar/kecil, bonus, diskon berjenjang), menambahkan catatan untuk admin bila perlu, lalu menandai order siap dikirim |
| **Manfaat bisnis** | Order masuk sistem tanpa menunggu sales kembali ke kantor; detail qty, bonus, dan diskon sudah tercatat sesuai praktik distributor |

**Yang tersedia dalam kapabilitas ini**

- Order offline (buat, edit, selesai / buka kembali)
- Qty besar–kecil, bonus, diskon berjenjang hingga 4 tingkat
- Catatan untuk admin; atribusi salesman; terikat cabang/depot
- Status: dikerjakan → siap sync → terkirim

---

## 3.2 Manajemen Kunjungan Pelanggan

| | |
|--|--|
| **Tujuan bisnis** | Membuktikan bahwa sales berada di lokasi outlet pada saat kunjungan |
| **Penggunaan operasional** | Sales check-in ke pelanggan dalam radius 100 m; melihat kunjungan aktif dan lama waktu; menutup kunjungan manual atau otomatis saat check-in ke outlet lain; meninjau riwayat per tanggal |
| **Manfaat bisnis** | Manajemen dapat melihat apakah kunjungan benar-benar dilakukan di dekat outlet, serta kapan kunjungan dibuka dan ditutup |

**Yang tersedia dalam kapabilitas ini**

- Check-in GPS (radius 100 meter)
- Indikator kunjungan aktif dan waktu berjalan
- Check-out manual atau otomatis
- Riwayat kunjungan per tanggal (termasuk status sync)

> **Catatan praktis:** Check-in membutuhkan pin GPS pelanggan. Outlet tanpa koordinat tidak muncul di daftar check-in terdekat.

---

## 3.3 Informasi Pelanggan di Lapangan

| | |
|--|--|
| **Tujuan bisnis** | Memberi sales akses data pelanggan di perangkat, termasuk lokasi outlet |
| **Penggunaan operasional** | Sales mencari pelanggan offline (wilayah dan alamat), membuka lokasi di Google Maps bila pin sudah ada, atau mendaftarkan koordinat GPS untuk pertama kali jika pin belum ada |
| **Manfaat bisnis** | Sales dapat menemukan dan menuju outlet dengan data yang tersedia; koordinat yang belum lengkap dapat dilengkapi dari lapangan dan dikirim ke sistem pusat |

**Yang tersedia dalam kapabilitas ini**

- Cari pelanggan offline (wilayah, alamat)
- Buka lokasi di Google Maps
- Registrasi GPS pertama kali + pengiriman ke sistem pusat

> **Catatan praktis:** Aplikasi tidak membuat pelanggan baru. Master pelanggan diunduh dari sistem pusat. Registrasi GPS bersifat sekali tulis untuk pin yang belum ada.

---

## 3.4 Produktivitas Tim Sales

| | |
|--|--|
| **Tujuan bisnis** | Memungkinkan sales menyelesaikan pekerjaan order di lapangan tanpa bergantung koneksi terus-menerus |
| **Penggunaan operasional** | Setelah data master diunduh, sales mencari produk, melihat informasi stok (tampil saja), membuat order, dan meninjau ringkasan order lokal (jumlah order, omzet kotor, total item) per hari |
| **Manfaat bisnis** | Sales dapat membuat order langsung dari lokasi pelanggan; sales dapat memantau hasil order hariannya di perangkat sendiri |

**Yang tersedia dalam kapabilitas ini**

- Kerja order dan kunjungan offline setelah sync master
- Cari produk (nama/kode, kategori, harga); tampilan stok (informasi saja)
- Ringkasan order lokal: jumlah order, omzet kotor, total item

> **Catatan praktis:** Order tidak mereservasi atau menyesuaikan stok.

---

## 3.5 Sinkronisasi dan Integrasi Operasional

| | |
|--|--|
| **Tujuan bisnis** | Memindahkan hasil kerja lapangan ke sistem operasional distributor |
| **Penggunaan operasional** | Sales memilih cabang/depot, mengunduh data master, lalu mengunggah order yang siap dikirim, catatan kunjungan, dan koordinat pelanggan yang baru dilengkapi |
| **Manfaat bisnis** | Kantor menerima order dan jejak kunjungan dari lapangan dalam konteks cabang yang benar, untuk diproses lebih lanjut di sistem distributor |

**Yang tersedia dalam kapabilitas ini**

- Login Google; pilih depot Jogja atau Magelang
- Unduh master (barang, pelanggan, salesman)
- Unggah order siap kirim, kunjungan, dan GPS pelanggan baru

> **Catatan praktis:** Sinkronisasi dipicu oleh sales. Kerja lapangan dapat offline; pengiriman ke sistem dilakukan saat ada jaringan.

---

# 4. Alur Kerja Operasional

Alur kerja yang didukung produk:

```text
Sales
  ↓
Login + pilih cabang/depot
  ↓
Sinkronisasi data master
  ↓
(Opsional) Daftarkan GPS outlet yang belum punya pin
  ↓
Kunjungi pelanggan
  ↓
Check-In GPS (dalam radius 100 m)
  ↓
Input order penjualan
  ↓
Check-Out (manual atau otomatis)
  ↓
Sinkronisasi order, kunjungan, dan lokasi
  ↓
Sistem Distributor
```

| Tahap | Penjelasan singkat |
|--------|---------------------|
| **Login + cabang** | Sales masuk dengan akun Google dan memilih depot tempat bekerja |
| **Sync master** | Data pelanggan, produk, dan salesman diunduh ke perangkat untuk kerja offline |
| **Daftar GPS (opsional)** | Outlet tanpa pin dapat dilengkapi koordinat dari lapangan |
| **Check-In** | Sales membuktikan keberadaan di dekat outlet (radius 100 m) |
| **Input order** | Sales mencatat permintaan pelanggan (qty, bonus, diskon, catatan admin) |
| **Check-Out** | Kunjungan ditutup manual, atau otomatis saat check-in ke outlet berikutnya |
| **Sinkronisasi** | Order siap kirim, kunjungan, dan GPS baru dikirim ke sistem distributor |

> **Catatan operasional**
>
> - Tidak ada modul rencana rute atau jadwal kunjungan di aplikasi.
> - Order dapat dibuat tanpa check-in terbuka; pola kerja yang didukung adalah kunjungan bersama pengambilan order.
> - Hanya satu kunjungan terbuka per sales; check-in baru menutup kunjungan sebelumnya.

---

# 5. Manfaat bagi Manajemen Distributor

| Manfaat | Penjelasan operasional |
|---------|-------------------------|
| **Disiplin kunjungan** | Check-in hanya dapat dilakukan jika sales berada dekat pin outlet (radius 100 m). Manajemen mendapat jejak kunjungan yang terkait lokasi, bukan hanya laporan lisan. |
| **Kecepatan pencatatan order** | Sales membuat order di lokasi pelanggan. Setelah order ditandai siap, data dapat diunggah ke sistem tanpa menunggu sales kembali ke kantor. |
| **Akurasi data pelanggan** | Outlet yang belum memiliki koordinat dapat dilengkapi dari lapangan. Data lokasi masuk ke master pusat untuk keperluan kunjungan dan navigasi. |
| **Pengurangan pekerjaan administratif** | Mengurangi ketergantungan pada catatan kertas dan pengetikan ulang order di kantor. Catatan untuk admin dapat dilampirkan langsung pada order. |
| **Visibilitas aktivitas sales** | Order dan kunjungan yang tersinkron tersedia di sisi sistem distributor untuk ditinjau back-office. Sales juga dapat melihat ringkasan order lokalnya sendiri. |

---

# 6. Ruang Lingkup Produk Saat Ini

## 6.1 Termasuk Dalam Produk

| Area | Kapabilitas yang tersedia |
|------|---------------------------|
| **Order** | Order offline; qty besar/kecil; bonus; diskon berjenjang (hingga 4 tingkat); catatan admin; status siap sync / terkirim; atribusi salesman |
| **Kunjungan** | Check-in GPS (radius 100 m); kunjungan aktif; check-out manual/otomatis; riwayat per tanggal |
| **Pelanggan** | Cari pelanggan offline; buka Maps; registrasi GPS pertama kali |
| **Produk** | Cari produk; lihat harga/kategori; tampilan stok (informasi saja) |
| **Sync & cabang** | Unduh master; unggah order, kunjungan, dan GPS; pilihan depot Jogja / Magelang |
| **Pemantauan lokal** | Ringkasan order di perangkat (jumlah, omzet kotor, item) |
| **Akses** | Login Google untuk sales lapangan |

## 6.2 Belum Termasuk Dalam Produk

Kapabilitas berikut **tidak** tersedia di SalesGO saat ini:

| Kapabilitas | Keterangan |
|-------------|------------|
| **Collection Management** | Tidak ada pencatatan pembayaran / penagihan |
| **Piutang / AR** | Tidak ada tampilan invoice outstanding atau piutang |
| **Delivery Execution** | Tidak ada alur pengiriman / pengantaran barang |
| **Retur** | Tidak ada proses retur di aplikasi |
| **Route Planning** | Tidak ada rencana rute atau jadwal kunjungan terjadwal |
| **Trade Promotion Management** | Tidak ada mesin promo; hanya diskon manual per baris |
| **Survey** | Tidak ada modul survei lapangan |
| **Photo / Signature Capture** | Tidak ada pengambilan foto atau tanda tangan |
| **Cetak / WhatsApp** | Tidak ada pencetakan struk atau berbagi via WhatsApp |
| **Pembuatan pelanggan baru** | Master pelanggan hanya diunduh dari sistem pusat |
| **Reservasi / penyesuaian stok** | Stok hanya ditampilkan; order tidak memotong stok |

> **Implikasi untuk keputusan manajemen:** SalesGO relevan untuk eksekusi order dan kunjungan lapangan. Kebutuhan penagihan, pengiriman, retur, atau perencanaan rute perlu ditangani di sistem / proses lain.

---

# 7. Gambaran Implementasi

Konsep aliran data secara sederhana:

```text
Aplikasi Android Sales
        ↓
    Cloud API
        ↓
 Sistem Distributor (BTR)
```

| Komponen | Peran |
|----------|--------|
| **Aplikasi Android Sales** | Tempat sales bekerja di lapangan: order, kunjungan, lokasi |
| **Cloud API** | Penghubung unduh data master dan unggah hasil kerja lapangan |
| **Sistem Distributor** | Penerima order, kunjungan, dan koordinat outlet untuk proses operasional lanjutan |

Sales bekerja di perangkat mobile. Setelah sinkronisasi, data order, kunjungan, dan lokasi outlet tersedia di sisi operasional distributor.

---

# 8. Ringkasan

SalesGO menempati posisi sebagai alat **eksekusi sales lapangan** dalam operasi distributor.

| Fokus produk | Yang dimaksud secara operasional |
|--------------|----------------------------------|
| **Sales execution** | Sales mencatat order di lokasi pelanggan, offline maupun online |
| **Customer visit execution** | Kunjungan dibuka dan ditutup dengan bukti lokasi GPS |
| **Order capture** | Permintaan pelanggan masuk sebagai order terstruktur ke sistem |
| **Operational reporting** | Ringkasan order lokal di perangkat; jejak order dan kunjungan tersinkron ke kantor |

**Kesimpulan untuk manajemen**

SalesGO relevan jika organisasi distributor ingin:

1. Sales dapat membuat order di lapangan tanpa menunggu kembali ke kantor
2. Kunjungan pelanggan dapat diverifikasi melalui keberadaan di dekat outlet
3. Lokasi outlet yang belum lengkap dapat dilengkapi dari lapangan
4. Hasil kerja lapangan masuk ke sistem distributor untuk diproses lebih lanjut

SalesGO **tidak** menggantikan modul piutang, pengiriman, retur, atau gudang. Ruang lingkupnya terbatas pada order lapangan, kunjungan, dan sinkronisasi data terkait ke sistem pusat.

---

*Dokumen ini disusun sebagai Profil Kapabilitas Produk untuk audiens manajemen. Sumber kapabilitas: analisis inventory produk SalesGO.*
