# Investigasi Provider AI dengan API Gratis untuk OpenCode

**Tanggal Verifikasi:** 17 September 2026 (UTC)  
**Tujuan:** Menemukan provider yang menyediakan akses API resmi dengan API Key / Access Token yang dapat digunakan aplikasi pihak ketiga (OpenCode, Cursor, Continue, Claude Code, dll) dengan kuota gratis yang masih aktif.  
**Metode Verifikasi:** Hanya sumber resmi provider (docs, pricing, rate-limits). Klaim pihak ketiga ditandai `[Tidak Terverifikasi Resmi]`.

---

## Daftar Isi

1. [Executive Summary](#1-executive-summary)
2. [Tabel Lengkap Provider Terverifikasi](#2-tabel-lengkap-provider-terverifikasi)
3. [Provider Paling Menarik untuk Pengguna OpenCode](#3-provider-paling-menarik-untuk-pengguna-opencode)
4. [Provider dengan Akses Frontier Models Gratis](#4-provider-dengan-akses-frontier-models-gratis)
5. [Tanggal Verifikasi](#5-tanggal-verifikasi)
6. [URL Sumber Resmi per Entri](#6-url-sumber-resmi-per-entri)
7. [Informasi Tidak Dapat Diverifikasi Secara Resmi](#7-informasi-tidak-dapat-diverifikasi-secara-resmi)

---

## 1. Executive Summary

*   **Free Tier Permanen benar-benar ada, tapi terkonsentrasi di 6 provider:** Google Gemini (`ai.google.dev`), Groq (`console.groq.com`), Mistral AI (`console.mistral.ai`), Cohere (`docs.cohere.com`), Z.ai GLM (`api.z.ai`), dan OpenRouter (`openrouter.ai`). Keenamnya **tidak butuh kartu kredit** untuk mulai dan diperbarui harian/bulanan, bukan kredit sekali pakai.
*   **Kredit Trial sekali pakai masih ada tapi memburuk:** Cerebras `$5/30 hari` (wajib kartu), Fireworks `$1`, Anthropic `$5/14 hari` (wajib verifikasi SMS), Alibaba Model Studio `1M token/model / 90 hari`, NVIDIA Build `1000-5000 credits`, semuanya habis dan tidak diperbarui. DeepSeek `5M token` **tidak lagi dijamin** di dokumen resmi 2026.
*   **Trend 2025-2026: Free tier ditarik:** `Together AI` menghapus free trial Juli 2025 (sekarang minimal `$5` prepaid), `SambaNova` menghapus free tier 14 Agustus 2026 (sekarang `402 PAYMENT_METHOD_REQUIRED`), `GitHub Models` **pensiun total 30 Juli 2026**, `Perplexity $5/bulan Pro` dihentikan **12 Februari 2026**.
*   **Implikasi untuk OpenCode (OpenAI Compatible + Tool Calling + Streaming):** Semua 6 provider permanen mendukung endpoint OpenAI-compatible. Groq bahkan punya dokumen khusus `console.groq.com/docs/coding-with-groq/opencode` untuk OpenCode/Cline/Roo. Z.ai unik mendukung **Anthropic-compatible** (`/api/anthropic`) sehingga bisa drop-in ke Claude Code.
*   **Frontier models gratis?** Ya tapi tidak merata. Gemini 2.5 Flash/Pro, Groq (Llama 3.3 70B, Qwen 3.8 27B, gpt-oss-120B), Mistral Large 3 / Codestral, Z.ai GLM-4.7-Flash (~200K context), Qwen3-Coder via Alibaba tersedia gratis. GPT-5, Claude 4.x, Grok 4 tidak ada free tier permanen — hanya lewat kredit trial/startup grant.

---

## 2. Tabel Lengkap Provider Terverifikasi

> Definisi kolom sesuai permintaan: Provider | URL | Model | API Available | Free Access Type | Free Quota | Expiration Date | Requires Credit Card | OpenAI Compatible | Notes

### 2.1 Free Tier Permanen (Tanpa Expiry)

| Provider | URL Resmi | Model Gratis Terverifikasi | API Available | Free Access Type | Free Quota | Expiration Date | Requires Credit Card | OpenAI Compatible | Notes |
|---|---|---|---|---|---|---|---|---|---|
| **Google Gemini (AI Studio)** | `https://ai.google.dev/gemini-api/docs/pricing` <br> `https://ai.google.dev/gemini-api/docs/rate-limits` | Gemini 2.5 Flash, Flash-Lite, 2.0 Flash | Ya | Permanent Free Tier | **$0 input/output.** RPD: 20 req/hari (Flash) / 500 RPD (Flash-Lite). Tanpa batas token terpisah, dibatasi RPM/TPM. Reset harian midnight PT. | Tidak ada expiry | Tidak | Ya (`https://generativelanguage.googleapis.com/v1beta/openai/`) | Paling murah hati. Konten free tier boleh dipakai Google untuk improvement. Grounding/search hanya di paid tier. |
| **Groq** | `https://console.groq.com/docs/rate-limits` <br> `https://console.groq.com/docs/billing-faqs` | `llama-3.3-70b-versatile`, `llama-3.1-8b-instant` (14,400 RPD), `qwen/qwen3.8-27b`, `openai/gpt-oss-120b/20b`, `compound`, `whisper-large-v3` | Ya | Permanent Free Tier | Per-model: `gpt-oss-120b`: 30 RPM / 1,000 RPD / 8K TPM / 200K TPD. `llama-3.1-8b-instant`: 30 RPM / **14,400 RPD** / 6K TPM / 500K TPD. | Tidak ada expiry | Tidak | Ya (`https://api.groq.com/openai/v1`) | Rekomendasi utama untuk coding. Docs resmi ada integrasi OpenCode. Limits per organization, bukan per key. |
| **Mistral AI (La Plateforme / Studio)** | `https://docs.mistral.ai/` <br> `https://help.mistral.ai/en/articles/455206-how-can-i-try-the-api-for-free-with-the-experiment-plan` | Seluruh lineup: Mistral Large 3, Medium 3.5, Small 4, Codestral, Ministral, Pixtral | Ya | Permanent Free Tier | **Experiment Plan: ~1 Miliar token/bulan**, ~1 req/detik. Angka resmi tidak dipublish, cek `Admin Console → Limits`. | Tidak ada expiry | Tidak (hanya verifikasi SMS) | Ya | Free tier request boleh dipakai untuk training kecuali di-opt-out di Console. Ideal untuk evaluasi Codestral. Startup grant `Mistralship` s/d $30K terpisah. |
| **Z.ai (Zhipu AI / GLM)** | `https://api.z.ai/` <br> `https://docs.z.ai/` | **GLM-4.7-Flash** (200K context, coding), **GLM-4.5-Flash** (128K), **GLM-4.6V-Flash** (vision) | Ya | Permanent Free Tier | **$0/$0 input/output selamanya** untuk 3 model Flash tersebut. Limit: **1 concurrent request (~1 req/detik)**. | Tidak ada expiry | Tidak | Ya + Anthropic Compatible (`/api/paas/v4` & `/api/anthropic`) | Hidden gem untuk coding. 200K konteks gratis langka. FlashX berbayar ($0.07/1M) untuk concurrency. |
| **Cohere** | `https://docs.cohere.com/docs/rate-limits.mdx` <br> `https://cohere.com/pricing` | Command A, Command R+, R, R7B, North Mini Code, Embed, Rerank | Ya | Permanent Free Tier | **Trial Key: 1,000 API calls/bulan** + 20 RPM (Chat), 10 RPM (Rerank), 2,000 inputs/min (Embed). | Tidak ada expiry (selama trial key) | Tidak | Tidak (Cohere SDK, tapi bisa via OpenRouter) | `Trial keys not for production/commercial`. Upgrade ke Production Key = pay-as-you-go. Command A Reasoning/Vision tetap trial-like walau production. |
| **OpenRouter** | `https://openrouter.ai/docs/api_reference/limits` <br> `https://openrouter.ai/docs/faq` | `openrouter/free` (router), 28+ model `:free` (Llama, Mistral, Qwen, Gemini Flash:free, dll - rotasi) | Ya | Permanent Free Tier | **20 RPM / 50 RPD → 1,000 RPD jika lifetime purchase ≥$10.** + `small free allowance` awal (jumlah tidak dipublish). | Daily reset UTC | Tidak (tapi saldo negatif = block 402) | Ya | Aggregator. `openrouter/free` otomatis pilih model free yang support tool/image. Best-effort, tidak untuk production. |
| **Hugging Face Inference Providers** | `https://huggingface.co/docs/inference-providers/main/en/pricing` | 200+ model via routed providers (DeepSeek, Qwen, Llama, Flux, dll) + `hf-inference` (CPU) | Ya | Permanent Free Tier | **Free users: $0.10/bulan, PRO: $2.00/bulan** (compute credits). | Reset bulanan | Tidak (untuk $0.10), PRO $9/bulan butuh kartu | Ya (`https://router.huggingface.co/v1`) | Kredit kecil tapi bisa switch provider tanpa ganti key. `Custom Provider Key` = billing langsung ke provider, tidak dapat kredit HF. |
| **NVIDIA Build (NIM)** | `https://build.nvidia.com/` <br> `https://build.nvidia.com/settings` | 100+ model: Llama 3.1 8B/70B, DeepSeek-R1 671B, Mixtral, Nemotron, MiniMax-M3 | Ya | Permanent Free Tier (Trial) | **1,000 credits on signup (+4,000 via request = total 5,000)** + **40 RPM per model**. | Tidak ada expiry time, habis jika credits habis | Tidak | Ya (`https://integrate.api.nvidia.com/v1`, key `nvapi-`) | Beberapa model kecil tetap bisa setelah credits habis. Forum 2025: sistem kredit diganti pure rate-limit trial. |

### 2.2 Trial Credit / Promo (Ada Expiry)

| Provider | URL Resmi | Model Gratis Terverifikasi | API Available | Free Access Type | Free Quota | Expiration Date | Requires Credit Card | OpenAI Compatible | Notes |
|---|---|---|---|---|---|---|---|---|---|
| **Alibaba Cloud Model Studio (DashScope)** | `https://www.alibabacloud.com/help/en/model-studio/new-free-quota` | Qwen3 family (Plus, Max, Coder, VL), Wan, Qwen-Image | Ya | Trial Credit | **~1,000,000 token per model** (tidak shared antar versi: `qwen-plus` ≠ `qwen-plus-latest`) + 100 image + 50s video | **90 hari** sejak aktivasi (setelah 8 Sep 2025) / 30 hari untuk aktivasi lama | Tidak | Ya (`https://dashscope-intl.aliyuncs.com/compatible-mode/v1`) | Hanya region Singapore (International). Ada toggle `Free Quota Only` agar tidak tertagih. |
| **Fireworks AI** | `https://fireworks.ai/pricing` | 50+ model: Llama, Qwen, DeepSeek, Mixtral, GLM, Flux, Whisper | Ya | Trial Credit | **$1 one-time credits** | Sampai habis, tidak ada expiry publish `[Tidak Terverifikasi]` | Tidak (10 RPM tanpa kartu, 600 RPM dengan kartu) | Ya (`https://api.fireworks.ai/inference/v1`) | Suspend jika $1 habis tanpa kartu. Startup credits s/d $10K via apply. |
| **Anthropic Claude** | `https://www.anthropic.com/pricing` <br> `https://console.anthropic.com/settings` | Claude Haiku/Sonnet/Opus (semua model API) | Ya | Trial Credit | **$5 one-time** | **14 hari setelah klik Claim** (versi lain claim 3 bulan `[Tidak Terverifikasi]`) | Tidak (tapi SMS verification, VoIP/Google Voice ditolak, UK excluded) | Tidak (tapi bisa via OpenRouter/AWS Bedrock) | Free tier 5 RPM. $5 ≈ 6M input token Haiku atau 330K Opus. Startup program s/d $25K terpisah. |
| **Cerebras** | `https://inference-docs.cerebras.ai/support/rate-limits` <br> `https://www.cerebras.ai/pricing` | `gpt-oss-120b`, `zai-glm-4.7`, `gemma-4-31b`, `qwen-3.8-27b` | Ya | Trial Credit | **$5 credits** + **5 RPM / 30K uncached TPM / 90K total TPM / 1M TPH / 1M TPD** | **30 hari** setelah grant | **Ya, wajib kartu** (tanpa kartu API inactive) | Ya | Bukan free tier permanen. Fastest WSE-3. Setelah habis harus purchase → Developer (1,000 RPM). |
| **ByteDance Doubao / Seed** | `https://www.volcengine.com/docs/82379/` & `https://docs.byteplus.com/en/docs/ModelArk/` | Seed 2.0 Pro/Lite/Mini/Code, Doubao 1.5 | Ya | Trial Credit | **500,000 token per eligible model** | Sampai habis `[Tidak Terverifikasi: tidak publish expiry]` | Tidak (tapi Volcengine butuh nomor HP China + real-name) | Ya | BytePlus (Internasional) minta enterprise info. Ada `Free Tokens Only` mode. |
| **DeepSeek Platform** | `https://api-docs.deepseek.com/quick_start/pricing` | `deepseek-chat` (V4 Flash), `deepseek-reasoner` (R1) | Ya | Trial Credit (Tidak Dijamin) | Klaim **5M token (~$8)** `[Tidak Terverifikasi Resmi]` – docs resmi hanya sebut `granted balance` tanpa jumlah universal | **30 hari** `[Tidak Terverifikasi]` | Tidak | Ya (`https://api.deepseek.com`) | Dashboard = source of truth. Jika tidak ada granted balance, langsung pay-as-you-go ($0.14/1M input Flash). |
| **xAI Grok** | `https://docs.x.ai/developers/rate-limits` <br> `https://console.x.ai/` | Grok 4, Grok 4 Fast, Grok 4.1 | Ya | Trial Credit + Beta Program | Promo **$25 one-time** `[Sudah tidak dipublish Aug 2026]` + **Data-sharing: ~$150/bulan** `[Reported, tidak dipublish resmi]` – butuh spend $5 dulu, opt-in team-level | Promo 30-90 hari, data-sharing reset bulanan | Terkadang Ya (deposit $5) | Ya (`https://api.x.ai/v1`) | Tidak ada permanent free tier. Quickstart sekarang suruh `load credits before use`. |

### 2.3 Provider yang TIDAK LAGI Memenuhi Kriteria

| Provider | Status per 17 Sep 2026 | URL Bukti | Keterangan |
|---|---|---|---|
| **Together AI** | Tidak ada free trial | `https://docs.together.ai/docs/billing-credits` | `Together AI does not currently offer free trials. Access requires minimum $5 credit purchase.` |
| **SambaNova Cloud** | Free tier dihapus | `https://cloud.sambanova.ai/pricing` | 14 Aug 2026: `Add a payment method and purchase credits` + `402 PAYMENT_METHOD_REQUIRED`. Dulu 20 RPD/200K TPD. |
| **GitHub Models** | Pensiun total | `https://docs.github.com/en/billing/concepts/product-billing/github-models` | 30 Juli 2026: `The playground, catalog, inference API, and BYOK are no longer available` |
| **OpenAI API** | Tidak ada permanent free tier | `https://platform.openai.com/docs/guides/rate-limits` | Trial $5 phased out mid-2025. Sisa: data-sharing free tokens ~1M/hari (opt-in) atau startup grant $2.5K via VC. |
| **Perplexity API** | $5/bulan Pro dihentikan | `https://docs.perplexity.ai/` | 12 Feb 2026 support confirmed `time-limited benefit discontinued`. |

---

## 3. Provider Paling Menarik untuk Pengguna OpenCode

Prioritas berdasarkan: **OpenAI-compatible, tool calling, streaming, konteks besar, tanpa kartu.**

### Tier S — Daily Driver (Pasang Pertama)

**1. Groq**
Alasan: Free tier paling stabil untuk coding loop, 14,400 RPD untuk `llama-3.1-8b-instant` (workhorse), `qwen/qwen3.8-27b` & `gpt-oss-120b` untuk reasoning. Latensi terendah (LPU 400+ tok/s), dokumentasi OpenCode eksplisit di `console.groq.com/docs/coding-with-groq/opencode`, tidak pakai data untuk training.
```
base_url: https://api.groq.com/openai/v1
```

**2. Google Gemini**
Alasan: 500 RPD untuk Flash-Lite = ratusan percakapan coding/hari. Konteks 1M, gratis selamanya, tanpa kartu. Cocok untuk long-file refactor.
```
base_url: https://generativelanguage.googleapis.com/v1beta/openai/
```

**3. Z.ai GLM-4.7-Flash**
Alasan: **200K konteks gratis selamanya** dengan tuning coding/agent. 1 req/detik cukup untuk single-dev workflow OpenCode. Dual kompatibilitas OpenAI + Anthropic = bisa untuk Claude Code juga.
```
base_url: https://api.z.ai/api/paas/v4
# Anthropic-compatible:
base_url: https://api.z.ai/api/anthropic
```

### Tier A — Evaluasi & Fallback

**4. Mistral AI —** `Codestral` & `Devstral` adalah model code SOTA Eropa, free tier ~1B token/bulan ideal untuk coba. Perhatikan opt-out training di Console.

**5. OpenRouter `:free` —** Router `openrouter/free` otomatis pilih model free yang support tool calling. 50 RPD (1,000 jika deposit $10) kecil tapi berguna untuk benchmark lintas model tanpa ganti key.
```
model: openrouter/free
# atau: meta-llama/llama-3.3-70b-instruct:free
```

**6. Alibaba Qwen —** `qwen3-coder-plus` (1M token/model gratis) sangat kuat untuk coding, pricelist setelah gratis juga murah.

### Tier B — Kredit Sekali Pakai

**7. Cerebras (butuh kartu)** — Jika butuh speed ekstrem `2,000 tok/s`, pakai $5 untuk test `gpt-oss-120b`, lalu off.

**8. NVIDIA Build (tanpa kartu)** — 1000-5000 credits untuk akses 100+ model open-weight, bagus untuk benchmark sebelum deploy ke Groq.

**Hindari untuk daily OpenCode:** Cohere (bukan OpenAI format, limit 1,000/bln kecil), Fireworks $1 (terlalu kecil), DeepSeek promo tidak pasti.

**Rekomendasi Setup OpenCode:**
```json
// opencode.json - contoh multi-provider
{
  "providers": {
    "groq": { "baseUrl": "https://api.groq.com/openai/v1", "model": "qwen/qwen3.8-27b" },
    "gemini": { "baseUrl": "https://generativelanguage.googleapis.com/v1beta/openai/", "model": "gemini-2.5-flash" },
    "zai": { "baseUrl": "https://api.z.ai/api/paas/v4", "model": "glm-4.7-flash" }
  }
}
```

---

## 4. Provider dengan Akses Frontier Models Gratis

| Frontier Model Family | Provider Gratis Terverifikasi | Model ID Gratis |
|---|---|---|
| **Gemini (Google)** | Google Gemini API | `gemini-2.5-flash`, `gemini-2.5-flash-lite`, `gemini-1.5-flash` |
| **Qwen (Alibaba)** | Alibaba Model Studio, Groq, Hugging Face | `qwen3-coder-plus`, `qwen/qwen3.8-27b`, `qwen3.6-plus` |
| **Mistral** | Mistral La Plateforme | `mistral-large-3`, `codestral-latest`, `mistral-small-4` |
| **GLM (Zhipu)** | Z.ai | `glm-4.7-flash` (200K), `glm-5.2` (via trial bukan free selamanya) |
| **DeepSeek** | DeepSeek Platform `[Trial, tidak dijamin]`, Groq (distill), NVIDIA | `deepseek-v3.2`, `deepseek-r1-distill` |
| **Kimi (Moonshot)** | Groq (`moonshotai/kimi-k2-instruct` 60 RPM/1K RPD di Groq free) | `k2-instruct` |
| **GPT (OpenAI open)** | Groq, Cerebras, NVIDIA | `openai/gpt-oss-120b/20b` (model open-weight, bukan GPT-5) |
| **Grok (xAI)** | **Tidak ada free permanen** – hanya via kredit data-sharing `$150/bulan` `[Tidak Terverifikasi]` | `grok-4`, `grok-4-fast` |
| **Claude (Anthropic)** | **Tidak ada free permanen** – hanya trial `$5` atau via OpenRouter `:free` (rotasi) | `claude-3.5-haiku:free` (via OpenRouter) |

> **Catatan:** `GPT-5`, `Claude 4.x Opus/Sonnet`, `Grok 4 Heavy` tidak tersedia di free tier permanen manapun per 17 Sep 2026. Akses gratis hanya via kredit startup/education/aggregator.

---

## 5. Tanggal Verifikasi

**17 September 2026 (UTC)** — Semua URL sumber resmi di-fetch/search pada tanggal ini. Rate limit dan harga dapat berubah tanpa pemberitahuan. Selalu cek dashboard akun masing-masing provider sebelum merencanakan workload produksi.

---

## 6. URL Sumber Resmi per Entri

| Provider | URL Sumber Resmi |
|---|---|
| Google Gemini | `https://ai.google.dev/gemini-api/docs/pricing` , `https://ai.google.dev/gemini-api/docs/rate-limits` |
| Groq | `https://console.groq.com/docs/rate-limits` , `https://console.groq.com/docs/billing-faqs` |
| Mistral AI | `https://docs.mistral.ai/` , `https://help.mistral.ai/en/articles/455206-how-can-i-try-the-api-for-free-with-the-experiment-plan` |
| Z.ai (Zhipu GLM) | `https://api.z.ai/api/paas/v4` , `https://z.ai/` , tracking `https://freellmapihub.com/p/zai-glm` |
| Cohere | `https://docs.cohere.com/docs/rate-limits.mdx` , `https://docs.cohere.com/docs/going-live.mdx` , `https://cohere.com/pricing` |
| OpenRouter | `https://openrouter.ai/docs/api_reference/limits` , `https://openrouter.ai/docs/faq` , `https://openrouter.ai/openrouter/free` |
| Hugging Face | `https://huggingface.co/docs/inference-providers/main/en/pricing` |
| NVIDIA Build | `https://build.nvidia.com/` , `https://build.nvidia.com/settings` |
| Alibaba Model Studio | `https://www.alibabacloud.com/help/en/model-studio/new-free-quota` |
| Fireworks AI | `https://fireworks.ai/pricing` |
| Anthropic Claude | `https://console.anthropic.com/` , `https://www.anthropic.com/pricing` |
| Cerebras | `https://inference-docs.cerebras.ai/support/rate-limits` , `https://www.cerebras.ai/pricing` |
| DeepSeek | `https://api-docs.deepseek.com/quick_start/pricing` |
| xAI Grok | `https://docs.x.ai/developers/rate-limits` , `https://console.x.ai/` |
| ByteDance Doubao/Seed | `https://www.volcengine.com/docs/82379/` , `https://docs.byteplus.com/en/docs/ModelArk/` |
| Together AI (negatif) | `https://docs.together.ai/docs/billing-credits` |
| SambaNova (negatif) | `https://cloud.sambanova.ai/pricing` |
| GitHub Models (negatif) | `https://docs.github.com/en/billing/concepts/product-billing/github-models` |
| OpenAI (negatif) | `https://platform.openai.com/docs/guides/rate-limits` |

---

## 7. Informasi Tidak Dapat Diverifikasi Secara Resmi

Flag `[Tidak Terverifikasi Resmi]` berarti tidak ditemukan di dokumentasi resmi provider per 17 Sep 2026 dan berasal dari tracker/aggregator pihak ketiga.

| Informasi | Status | Sumber Asal |
|---|---|---|
| **Mistral ~1 Miliar token/bulan** | Tidak ada angka eksak di docs publik; hanya di console per-workspace | `pricepertoken.com`, `freellmapihub.com` |
| **DeepSeek 5M token / 30 hari** | Tidak ada di `api-docs.deepseek.com/pricing`. Docs hanya menyebut `granted balance` tanpa jumlah universal | `aicreditmart.com`, `tokenmix.ai` |
| **xAI $25 signup + $150/bulan data-sharing** | Tidak lagi di `docs.x.ai` per 24 Aug 2026. Perkstack: `No longer published` | `perkstack.co`, `aitoolsrecap.com` |
| **Alibaba 500K Doubao expiry** | Tidak ada expiry di docs baru; tracker sebut `no official expiry` | `yangmao.ai` |
| **OpenAI $5 trial otomatis** | Docs tidak lagi menjamin; status bergantung region & waktu signup | `perkstack.co` |
| **Fireworks $1 expiry** | `Not stated officially` di official pricing | `buildaicurrent.com` |
| **Anthropic $5 = 14 hari vs 3 bulan** | Konflik antar sumber; official tidak publish durasi universal | Konsol Anthropic |

**Rekomendasi:** Selalu cek dashboard akun masing-masing (`Billing` / `Limits` / `Usage`) sebagai source of truth sebelum merencanakan workload. Jangan mengandalkan angka pihak ketiga untuk budgeting produksi.

---

*Dokumen dibuat sebagai Temporary Work sesuai `AGENTS.md` — akan dihapus setelah pengetahuan dipindahkan ke Permanent Knowledge jika diperlukan. Verifikasi ulang disarankan tiap 30 hari karena free tier sangat dinamis.*
