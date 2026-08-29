\# LOGISTIC TRANSPORT DOMAIN



\## Vision



\*\*Logistic Transport\*\* adalah bounded context yang bertanggung jawab mengelola seluruh aktivitas operasional transportasi distribusi barang, mulai dari perencanaan pengiriman hingga penyelesaian proses pengiriman kepada pelanggan.



Domain ini memastikan setiap Faktur yang telah diterbitkan oleh Sales dapat dikirim secara tepat, efisien, dan memiliki status operasional yang dapat dipantau selama proses distribusi.



Logistic Transport berfokus pada \*\*eksekusi pengiriman\*\*, bukan pada proses penjualan, pergudangan, ataupun penagihan.



\---



\# Business Mission



Mengubah sekumpulan Faktur menjadi aktivitas pengiriman yang terencana, terlaksana, dan terdokumentasi dengan baik.



\---



\# Domain Boundary



```text

&#x20;                SALES



&#x20;       Sales Order

&#x20;            │

&#x20;       Create Faktur

&#x20;            │

&#x20;            ▼

──────────────────────────────────────────

&#x20;         LOGISTIC TRANSPORT

──────────────────────────────────────────



Shipment Planning



Transportation



Delivery Execution



──────────────────────────────────────────

&#x20;            │

&#x20;            ▼



Collection / Finance

```



Domain ini dimulai setelah Faktur diterbitkan oleh Sales dan berakhir ketika seluruh aktivitas pengiriman telah selesai dicatat.



\---



\# Out of Scope



Logistic Transport \*\*tidak\*\* bertanggung jawab terhadap:



\### Sales



\* Sales Order

\* Faktur

\* Customer Order

\* Harga

\* Diskon



\### Warehouse Management



\* Picking

\* Packing

\* Barcode Picking

\* Cross-Faktur Picking

\* Warehouse Task

\* Stock Opname

\* Stock Adjustment

\* Inventory Control



\### Fleet Management



\* Master Kendaraan

\* Jadwal Servis

\* Maintenance

\* BBM

\* Ban

\* Pajak Kendaraan

\* SIM Driver



\### Finance



\* Collection

\* Payment

\* Piutang



Domain-domain tersebut hanya menyediakan data yang diperlukan oleh Logistic Transport.



\---



\# Business Capabilities



Phase pertama terdiri dari tiga capability.



\---



\# 1. Shipment Planning



\## Purpose



Shipment Planning bertanggung jawab menyusun rencana pengiriman berdasarkan Faktur yang telah diterbitkan.



Capability ini merupakan aktivitas perencanaan sebelum kendaraan berangkat.



Shipment Planning menjawab pertanyaan:



> "Besok kendaraan mana mengirim Faktur yang mana?"



\---



\## Responsibilities



Shipment Planning bertanggung jawab untuk:



\* memilih Faktur yang akan dikirim

\* mengelompokkan Faktur menjadi Shipment

\* menentukan tanggal pengiriman

\* menentukan kendaraan

\* menentukan driver

\* menentukan urutan pelanggan

\* mengatur prioritas pengiriman



Shipment Planning menghasilkan Shipment yang siap dieksekusi.



\---



\## Output



\* Shipment

\* Driver Assignment

\* Vehicle Assignment

\* Delivery Manifest

\* Delivery Sequence



\---



\# 2. Transportation



\## Purpose



Transportation bertanggung jawab mengeksekusi Shipment yang telah direncanakan.



Capability ini mengelola perjalanan kendaraan sejak keberangkatan hingga perjalanan selesai.



Transportation menjawab pertanyaan:



> "Shipment ini sedang berada pada tahap apa?"



\---



\## Responsibilities



Transportation mengelola:



\* keberangkatan kendaraan

\* perjalanan kendaraan

\* urutan kunjungan pelanggan

\* progres pengiriman

\* penyelesaian perjalanan



Transportation tidak mengubah isi Shipment.



Transportation hanya mengelola status pelaksanaan Shipment.



\---



\## Shipment Lifecycle (contoh)



\* Planned

\* Ready

\* Departed

\* In Delivery

\* Completed

\* Cancelled



\---



\## Output



Transportation menghasilkan informasi operasional seperti:



\* waktu berangkat

\* waktu selesai

\* status Shipment

\* progres pengiriman



\---



\# 3. Delivery Execution



\## Purpose



Delivery Execution bertanggung jawab mencatat hasil pengiriman pada setiap pelanggan.



Transportation mengelola perjalanan kendaraan.



Delivery Execution mengelola hasil setiap kunjungan pelanggan.



Capability ini menjawab pertanyaan:



> "Bagaimana hasil pengiriman ke pelanggan?"



\---



\## Responsibilities



Delivery Execution mencatat:



\* barang diterima

\* pengiriman sebagian

\* pengiriman gagal

\* pelanggan tutup

\* pelanggan menolak

\* barang dikembalikan

\* catatan driver



\---



\## Delivery Result



Contoh hasil Delivery:



\* Delivered

\* Partially Delivered

\* Failed

\* Returned



Beserta informasi pendukung seperti:



\* waktu tiba

\* waktu selesai

\* catatan pengiriman



\---



\# Core Business Object



\## Shipment



Shipment merupakan Aggregate Root sekaligus pusat koordinasi seluruh aktivitas Logistic Transport.



Shipment merepresentasikan satu misi pengiriman.



Shipment terdiri atas:



\* satu atau beberapa Faktur

\* satu kendaraan

\* satu driver

\* satu tanggal pengiriman

\* satu delivery sequence



Seluruh proses Transportation dan Delivery Execution selalu dilakukan terhadap Shipment.



\---



\# Business Workflow



```text

Sales

│

├── Create Faktur

│

▼



Shipment Planning



├── Select Faktur

├── Create Shipment

├── Assign Driver

├── Assign Vehicle

├── Arrange Delivery Sequence



▼



Transportation



├── Depart

├── Visit Customer

├── Monitor Progress



▼



Delivery Execution



├── Delivered

├── Partial Delivery

├── Failed

├── Returned



▼



Shipment Completed

```



\---



\# Primary Actors



Office



\* Logistics Planner



Field



\* Driver

\* Delivery Helper



Management



\* Logistics Supervisor

\* Owner



\---



\# Design Principles



\### Mobile First



Seluruh aktivitas operasional lapangan harus dapat dilakukan menggunakan aplikasi Android.



\---



\### Shipment-Centric



Shipment merupakan pusat seluruh aktivitas domain.



Seluruh capability bekerja terhadap Shipment.



\---



\### Execution-Oriented



Domain ini mengelola pelaksanaan distribusi barang, bukan transaksi bisnis.



\---



\### Pragmatic



Fokus pada penyelesaian kebutuhan operasional harian.



Optimisasi seperti route optimization, GPS tracking, fleet maintenance, atau predictive planning berada di luar ruang lingkup Phase-1.



\---



\### Extensible



Domain harus dapat berkembang tanpa mengubah fondasi model bisnis.



Capability berikut dapat ditambahkan pada fase selanjutnya:



\* Warehouse Operations

\* Stock Opname

\* Fleet Management

\* Proof of Delivery

\* Delivery Analytics

\* Route Optimization

\* Vehicle Tracking



\---



\## Catatan Desain



Saya ingin memberikan satu rekomendasi kecil yang menurut saya akan sangat membantu konsistensi model domain ke depan.



Daripada menggunakan nama capability \*\*Transportation\*\*, saya akan mempertimbangkan nama \*\*Transport Execution\*\*.



Sehingga tiga capability menjadi:



1\. \*\*Shipment Planning\*\*

2\. \*\*Transport Execution\*\*

3\. \*\*Delivery Execution\*\*



Ketiganya memiliki pola penamaan yang konsisten (\*\*Planning → Execution → Execution\*\*) dan memperjelas bahwa domain ini bukan mengelola armada kendaraan (\*transportation management\*), melainkan \*\*pelaksanaan transportasi\*\* (\*transport execution\*). Dengan demikian, jika suatu hari Anda membangun bounded context \*\*Fleet Management\*\*, tidak akan terjadi tumpang tindih terminologi antara pengelolaan armada dan pelaksanaan pengiriman.



