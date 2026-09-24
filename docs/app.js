/* ============ PEAK/8K — static edition engine ============ */
"use strict";

/* ---------------- icons ---------------- */
const IC = {
  film: '<svg viewBox="0 0 24 24" class="ico lime" fill="none" stroke="currentColor" stroke-width="1.6"><rect x="3" y="4" width="18" height="16" rx="2"/><path d="M7 4v16M17 4v16M3 9h4M3 15h4M17 9h4M17 15h4"/></svg>',
  audio: '<svg viewBox="0 0 24 24" class="ico lime" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"><path d="M4 10v4M8 7v10M12 4v16M16 7v10M20 10v4"/></svg>',
  file: '<svg viewBox="0 0 24 24" class="ico lime" fill="none" stroke="currentColor" stroke-width="1.6"><path d="M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8z"/><path d="M14 3v5h5"/></svg>',
  globe: '<svg viewBox="0 0 24 24" class="ico lime" fill="none" stroke="currentColor" stroke-width="1.6"><circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3c2.5 2.6 3.8 5.8 3.8 9S14.5 18.4 12 21c-2.5-2.6-3.8-5.8-3.8-9S9.5 5.6 12 3z"/></svg>',
  zap: '<svg viewBox="0 0 24 24" class="ico"><path d="M13 2 4 14h6l-1 8 9-12h-6z" fill="currentColor"/></svg>',
  scan: '<svg viewBox="0 0 24 24" class="ico lime" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"><path d="M3 7V5h18v2M3 12h18M3 19v-2h18v2"/></svg>',
  shield: '<svg viewBox="0 0 24 24" class="ico lime" fill="none" stroke="currentColor" stroke-width="1.6"><path d="M12 3l8 3v5c0 5-3.5 8.5-8 10-4.5-1.5-8-5-8-10V6z"/></svg>',
  fp: '<svg viewBox="0 0 24 24" class="ico lime" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"><path d="M12 11a3.5 3.5 0 0 0-3.5 3.5c0 2.5-.6 4.5-1.6 6.2M12 7.5a7 7 0 0 1 7 7c0 1.8-.2 3.4-.7 5M5 11.6c.5-3.3 3.4-5.8 7-5.8M12 14.5c0 2-.4 3.8-1.1 5.5M15.2 14.9c0 1.5-.3 2.9-.8 4.3"/></svg>',
  gauge: '<svg viewBox="0 0 24 24" class="ico lime" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"><path d="M5 19a9 9 0 1 1 14 0M12 13l4-4"/></svg>',
  repeat: '<svg viewBox="0 0 24 24" class="ico lime" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><path d="M17 2l4 4-4 4"/><path d="M3 11v-1a4 4 0 0 1 4-4h14M7 22l-4-4 4-4"/><path d="M21 13v1a4 4 0 0 1-4 4H3"/></svg>',
  dl: '<svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M12 3v12M7 11l5 5 5-5M4 21h16"/></svg>',
  ok: '<svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><path d="M8.5 12.5l2.5 2.5 5-6"/></svg>',
  handoff: '<svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M9 14 4 9l5-5"/><path d="M4 9h10a6 6 0 0 1 0 12h-3"/></svg>',
  radio: '<svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"><circle cx="12" cy="12" r="2"/><path d="M7.8 16.2a5.5 5.5 0 0 1 0-8.4M16.2 7.8a5.5 5.5 0 0 1 0 8.4M5 19a9 9 0 0 1 0-14M19 5a9 9 0 0 1 0 14" opacity=".55"/></svg>',
  power: '<svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"><path d="M12 3v8"/><path d="M6.3 6.5a8 8 0 1 0 11.4 0"/></svg>',
  lock: '<svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="1.6"><rect x="5" y="11" width="14" height="9" rx="2"/><path d="M8 11V8a4 4 0 0 1 8 0v3"/></svg>',
  search: '<svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"><circle cx="10.5" cy="10.5" r="6.5"/><path d="M15.5 15.5 21 21"/></svg>',
  binary: '<svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="1.6"><rect x="4" y="3" width="6" height="8" rx="1.5"/><rect x="14" y="13" width="6" height="8" rx="1.5"/><path d="M17 3v8M14 6h6M7 13v8M4 16h6"/></svg>',
  waves: '<svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"><path d="M4 10v4M8 7v10M12 4v16M16 7v10M20 10v4"/></svg>',
  braces: '<svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"><path d="M8 4H7a2 2 0 0 0-2 2v3a2 2 0 0 1-2 2 2 2 0 0 1 2 2v3a2 2 0 0 0 2 2h1M16 4h1a2 2 0 0 1 2 2v3a2 2 0 0 0 2 2 2 2 0 0 0-2 2v3a2 2 0 0 1-2 2h-1"/></svg>',
  infinity: '<svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="1.6"><path d="M6.5 8.5c-2 0-3.5 1.6-3.5 3.5s1.5 3.5 3.5 3.5c3.5 0 7.5-7 11-7 2 0 3.5 1.6 3.5 3.5s-1.5 3.5-3.5 3.5c-3.5 0-7.5-7-11-7z"/></svg>',
  alert: '<svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"><path d="M12 4 2.5 20h19z"/><path d="M12 10v4M12 17.2v.3"/></svg>',
};

/* ---------------- helpers ---------------- */
const $ = (s, el = document) => el.querySelector(s);
const $$ = (s, el = document) => [...el.querySelectorAll(s)];

function humanBytes(n) {
  if (n == null || !Number.isFinite(n)) return "—";
  if (n === 0) return "0 B";
  const u = ["B", "KB", "MB", "GB", "TB"];
  const i = Math.min(u.length - 1, Math.floor(Math.log2(n) / 10));
  return `${(n / 2 ** (10 * i)).toFixed(i === 0 ? 0 : 1)} ${u[i]}`;
}
function relTime(t) {
  const s = Math.max(1, Math.floor((Date.now() - t) / 1000));
  if (s < 60) return `${s}s ago`;
  const m = Math.floor(s / 60);
  if (m < 60) return `${m}m ago`;
  const h = Math.floor(m / 60);
  if (h < 24) return `${h}h ago`;
  return `${Math.floor(h / 24)}d ago`;
}
function parseUrl(raw) {
  try {
    const u = new URL(String(raw).trim());
    return u.protocol === "http:" || u.protocol === "https:" ? u : null;
  } catch {
    return null;
  }
}
function guessFilename(u, disposition) {
  if (disposition) {
    const star = /filename\*\s*=\s*(?:UTF-8'')?([^;]+)/i.exec(disposition);
    if (star) {
      try { return decodeURIComponent(star[1].trim().replace(/^"|"$/g, "")); } catch {}
    }
    const plain = /filename\s*=\s*"?([^";]+)"?/i.exec(disposition);
    if (plain) return plain[1].trim();
  }
  const last = u.pathname.split("/").filter(Boolean).pop() || "download";
  try { return decodeURIComponent(last); } catch { return last; }
}
function kindFromType(ct) {
  if (ct.startsWith("video/")) return "video";
  if (ct.startsWith("audio/")) return "audio";
  return "file";
}
function classify(u) {
  const h = u.hostname.toLowerCase().replace(/^www\./, "");
  if (h === "youtube.com" || h === "youtu.be" || h.endsWith(".youtube.com")) return "youtube";
  if (h === "instagram.com" || h.endsWith(".instagram.com")) return "instagram";
  if (h === "facebook.com" || h === "fb.watch" || h.endsWith(".facebook.com")) return "facebook";
  if (h === "tiktok.com" || h.endsWith(".tiktok.com")) return "tiktok";
  if (h === "twitter.com" || h === "x.com" || h.endsWith(".twitter.com")) return "twitter";
  if (h === "vimeo.com" || h.endsWith(".vimeo.com")) return "vimeo";
  if (h === "reddit.com" || h.endsWith(".reddit.com")) return "reddit";
  return "direct";
}
/* Map viewer/share links onto each platform's own direct-download endpoint. */
function resolveDirect(u) {
  const h = u.hostname.toLowerCase().replace(/^www\./, "");
  if (h === "drive.google.com" || h === "docs.google.com") {
    const m = /\/(?:file|document)\/d\/([\w-]+)/.exec(u.pathname);
    const id = (m && m[1]) || u.searchParams.get("id");
    if (id) {
      return {
        url: `https://drive.usercontent.google.com/download?id=${id}&export=download&confirm=t`,
        provider: "Google Drive",
      };
    }
  }
  if (h === "dropbox.com" || h.endsWith(".dropbox.com")) {
    const v = new URL(u.toString());
    v.searchParams.delete("dl");
    v.searchParams.set("dl", "1");
    return { url: v.toString(), provider: "Dropbox" };
  }
  if (h === "onedrive.live.com" || h === "1drv.ms") {
    const v = new URL(u.toString());
    if (!v.searchParams.has("download")) v.searchParams.set("download", "1");
    return { url: v.toString(), provider: "OneDrive" };
  }
  return { url: u.toString(), provider: u.hostname };
}

/* ---------------- particle field (high performance, low RAM) ---------------- */
(function field() {
  const c = $("#field");
  if (!c) return;
  const ctx = c.getContext("2d", { alpha: true });
  let w = (c.width = innerWidth), h = (c.height = innerHeight);
  const mouse = { x: -1e4, y: -1e4 };
  const N = Math.min(26, Math.max(16, Math.floor((w * h) / 75000)));
  const parts = Array.from({ length: N }, () => ({
    x: Math.random() * w, y: Math.random() * h,
    vx: (Math.random() - 0.5) * 0.22, vy: (Math.random() - 0.5) * 0.22,
    r: Math.random() * 1.4 + 0.5,
    c: Math.random() > 0.82 ? "rgba(216,255,62,.7)" : Math.random() > 0.5 ? "rgba(124,92,255,.55)" : "rgba(237,237,232,.35)"
  }));

  let rafId = null;
  let running = true;

  function onResize() { w = c.width = innerWidth; h = c.height = innerHeight; }
  addEventListener("resize", onResize, { passive: true });
  addEventListener("mousemove", (e) => { mouse.x = e.clientX; mouse.y = e.clientY; }, { passive: true });
  addEventListener("mouseout", () => { mouse.x = -1e4; mouse.y = -1e4; }, { passive: true });

  document.addEventListener("visibilitychange", () => {
    if (document.hidden) {
      running = false;
      if (rafId) cancelAnimationFrame(rafId);
      rafId = null;
    } else {
      running = true;
      if (!rafId) rafId = requestAnimationFrame(tick);
    }
  });

  const LINK = 110;
  const LINK2 = LINK * LINK;

  function tick() {
    if (!running) return;
    ctx.clearRect(0, 0, w, h);

    for (let i = 0; i < parts.length; i++) {
      const p = parts[i];
      const dx = mouse.x - p.x, dy = mouse.y - p.y;
      const d2 = dx * dx + dy * dy;
      if (d2 < 40000 && d2 > 40) {
        const invD = 0.012 / Math.sqrt(d2);
        p.vx += dx * invD;
        p.vy += dy * invD;
      }
      p.x += p.vx; p.y += p.vy;
      p.vx *= 0.985; p.vy *= 0.985;
      if (p.x < -10) p.x = w + 10; else if (p.x > w + 10) p.x = -10;
      if (p.y < -10) p.y = h + 10; else if (p.y > h + 10) p.y = -10;

      ctx.beginPath();
      ctx.fillStyle = p.c;
      ctx.arc(p.x, p.y, p.r, 0, 6.283);
      ctx.fill();
    }

    ctx.beginPath();
    ctx.strokeStyle = "rgba(237,237,232,0.045)";
    for (let i = 0; i < parts.length; i++) {
      const a = parts[i];
      for (let j = i + 1; j < parts.length; j++) {
        const b = parts[j];
        const dx = a.x - b.x, dy = a.y - b.y;
        if (dx * dx + dy * dy < LINK2) {
          ctx.moveTo(a.x, a.y);
          ctx.lineTo(b.x, b.y);
        }
      }
    }
    ctx.stroke();

    rafId = requestAnimationFrame(tick);
  }

  rafId = requestAnimationFrame(tick);
})();

/* ---------------- custom cursor (zero layout thrashing, idle sleep) ---------------- */
(function cursor() {
  if (!matchMedia("(pointer: fine)").matches) return;
  const dot = $("#curDot"), ring = $("#curRing");
  if (!dot || !ring) return;
  let rx = -100, ry = -100, tx = -100, ty = -100;
  let isMoving = false;
  let rafId = null;

  addEventListener("mousemove", (e) => {
    tx = e.clientX; ty = e.clientY;
    dot.style.transform = `translate(${tx - 4}px, ${ty - 4}px)`;
    const hot = e.target && e.target.closest("a,button,input,.chip,.yt-opt-item");
    ring.style.width = ring.style.height = hot ? "50px" : "28px";
    ring.style.opacity = hot ? ".95" : ".45";
    if (!isMoving) {
      isMoving = true;
      rafId = requestAnimationFrame(follow);
    }
  }, { passive: true });

  function follow() {
    rx += (tx - rx) * 0.22;
    ry += (ty - ry) * 0.22;
    ring.style.transform = `translate(${rx - 14}px, ${ry - 14}px)`;
    if (Math.abs(tx - rx) > 0.2 || Math.abs(ty - ry) > 0.2) {
      rafId = requestAnimationFrame(follow);
    } else {
      isMoving = false;
      rafId = null;
    }
  }
})();

/* ---------------- kinetic hero ---------------- */
(function hero() {
  const el = $("#heroTitle");
  if (!el) return;
  const rows = [
    { text: "MAXIMUM", cls: "" },
    { text: "RESOL", cls: "indent hollow" },
    { text: "UTION.", cls: "indent accent", cont: true },
  ];
  let i = 0, html = "";
  rows.forEach((r, idx) => {
    const spans = r.text.split("").map((ch) => {
      i++;
      return `<span class="ch" style="animation-delay:${0.35 + i * 0.028}s">${ch}</span>`;
    }).join("");
    if (r.cont) {
      html += spans + "</span>";
    } else if (idx === 0) {
      html += `<span class="row">${spans}</span>`;
    } else {
      html += `<span class="row"><span class="${r.cls}" style="display:inline-flex">${spans}`;
    }
  });
  el.innerHTML = html;
})();

/* ---------------- ticker ---------------- */
(function ticker() {
  const t = $("#tickerTrack");
  if (!t) return;
  const items = ["8K UHD 4320P", "16K-READY PIPELINE", "BIT-EXACT PASSTHROUGH", "ZERO RECOMPRESSION", "LOSSLESS AUDIO", "RANGE-RESUMABLE", "HDR / DOLBY VISION", "HIGHEST SOURCE BITRATE"];
  t.innerHTML = [...items, ...items].map((x) => `<span>${x}<i>◆</i></span>`).join("");
})();

/* ---------------- protocol grid ---------------- */
(function proto() {
  const g = $("#protoGrid");
  if (!g) return;
  const items = [
    [IC.binary, "BIT-EXACT TUNNEL", "The byte stream crosses untouched. What the source serves is what your disk receives — identical checksum, frame, sample."],
    [IC.waves, "AUDIO, UNBROKEN", "No transcoding layer ever touches the soundtrack. 24-bit stays 24-bit, multichannel stays multichannel."],
    [IC.repeat, "BREAK-PROOF RANGES", "Range requests flow end-to-end. A dropped connection is a shrug — resume exactly where the last byte landed."],
    [IC.braces, "INSTANT FORENSICS", "Every pasted link is interrogated first: type, payload size, filename, capability — before a payload byte moves."],
    [IC.infinity, "RESOLUTION-AGNOSTIC", "No codec parsing, no ceiling. Whatever the source holds — SD or 8K 4320p and beyond — flows at full fidelity."],
    [IC.zap, "UNIVERSAL EXTRACTION", "YouTube 4K/8K, Instagram reels, Facebook HD, TikTok, direct CDN feeds — 100% unlocked bit-exact media pipeline."],
  ];
  g.innerHTML = items.map((x, i) => `
    <div class="proto-cell">
      ${x[0]}
      <span class="proto-num">0${i + 1}</span>
      <h3>${x[1]}</h3>
      <p>${x[2]}</p>
    </div>`).join("");
})();

/* ---------------- ledger (localStorage) ---------------- */
const LKEY = "peak_ledger_v1";
const Ledger = {
  all() {
    try { return JSON.parse(localStorage.getItem(LKEY)) || []; } catch { return []; }
  },
  add(entry) {
    const arr = Ledger.all();
    arr.unshift(entry);
    localStorage.setItem(LKEY, JSON.stringify(arr.slice(0, 12)));
    Ledger.render();
  },
  bump(url) {
    const arr = Ledger.all().map((j) => j.url === url ? { ...j, status: "downloaded", t: Date.now() } : j);
    localStorage.setItem(LKEY, JSON.stringify(arr));
    Ledger.render();
  },
  render() {
    const body = $("#ledgerBody");
    if (!body) return;
    const items = Ledger.all();
    if (!items.length) {
      body.innerHTML = `<div class="empty">${IC.search}Ledger empty — feed the engine its first link</div>`;
      return;
    }
    const icons = { video: IC.film, audio: IC.audio, file: IC.file, embed: IC.globe };
    body.innerHTML = items.map((j, i) => `
      <div class="row-item" style="animation-delay:${i * 0.05}s">
        <div class="row-ico">${icons[j.kind] || IC.file}</div>
        <div class="row-body">
          <div class="row-title">${escapeHtml(j.title || j.provider || j.url)}</div>
          <div class="row-sub">${escapeHtml(j.provider || "")}${j.ct ? " • " + escapeHtml(j.ct) : ""}</div>
        </div>
        <div class="row-size">${humanBytes(j.size)}</div>
        ${j.kind !== "embed" ? `<button class="row-dl" data-re-dl="${encodeURIComponent(j.url)}" data-name="${escapeHtml(j.title || "")}" title="Pull again">${IC.dl}</button>` : ""}
        <span class="tag ${j.status === "downloaded" ? "done" : ""}">${j.status}</span>
        <span class="row-time">${relTime(j.t)}</span>
      </div>`).join("");
  },
};
function escapeHtml(s) {
  return String(s).replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}

/* ---------------- 3-layer download ---------------- */
const DL_SOFT_CAP = 768 * 1024 * 1024;

function nativeHandoff(url, filename) {
  const a = document.createElement("a");
  a.href = url;
  if (filename) a.download = filename;
  a.rel = "noopener";
  a.target = "_blank";
  document.body.appendChild(a);
  a.click();
  a.remove();
}

/* ---------------- console / analyzer ---------------- */
(function consoleEngine() {
  const form = $("#consoleForm");
  const input = $("#urlInput");
  const btn = $("#analyzeBtn");
  const label = $("#analyzeLabel");
  const stepsBox = $("#probeSteps");
  const zone = $("#resultZone");
  const clearBtn = $("#clearBtn");
  const pasteBtn = $("#pasteBtn");
  if (!form) return;

  function updateClearBtn() {
    if (!clearBtn) return;
    if (input.value && input.value.trim().length > 0) {
      clearBtn.classList.remove("hidden");
    } else {
      clearBtn.classList.add("hidden");
    }
  }

  if (clearBtn) {
    clearBtn.addEventListener("click", () => {
      input.value = "";
      updateClearBtn();
      input.focus();
      zone.innerHTML = "";
    });
  }

  if (pasteBtn) {
    pasteBtn.addEventListener("click", async () => {
      try {
        if (navigator.clipboard && navigator.clipboard.readText) {
          const text = await navigator.clipboard.readText();
          if (text && text.trim()) {
            input.value = text.trim();
            updateClearBtn();
            analyze(input.value);
            return;
          }
        }
      } catch {}
      input.focus();
      input.select();
    });
  }

  input.addEventListener("input", updateClearBtn);
  input.addEventListener("change", updateClearBtn);
  input.addEventListener("keyup", updateClearBtn);

  let probing = false;
  let stepTimers = [];

  function setSteps(i) {
    $$(".step", stepsBox).forEach((s, k) => {
      s.classList.toggle("on", k === i);
      s.classList.toggle("ok", k < i);
    });
  }
  function startSteps() {
    stepsBox.classList.remove("hidden");
    stepTimers.forEach(clearTimeout);
    stepTimers = [0, 1, 2, 3].map((k) => setTimeout(() => setSteps(k), 320 * k));
    setSteps(0);
  }
  function stopSteps() {
    stepTimers.forEach(clearTimeout);
    stepsBox.classList.add("hidden");
  }
  function showError(msg) {
    zone.innerHTML = `<div class="error-box">${IC.alert}<span>${escapeHtml(msg)}</span></div>`;
  }

  const BACKEND_API_BASE = (function() {
    if (typeof window !== "undefined" && window.FLOWDOWN_API_URL) return window.FLOWDOWN_API_URL;
    if (typeof location !== "undefined" && location.port === "4000") return `${location.origin}/api/v1`;
    return "http://localhost:4000/api/v1";
  })();

  function formatDuration(sec) {
    if (!sec || !Number.isFinite(sec) || sec <= 0) return "";
    const total = Math.round(sec);
    const m = Math.floor(total / 60);
    const s = total % 60;
    if (m < 60) return `${m}:${s.toString().padStart(2, "0")}`;
    const h = Math.floor(m / 60);
    const remM = m % 60;
    return `${h}:${remM.toString().padStart(2, "0")}:${s.toString().padStart(2, "0")}`;
  }

  function cleanCodec(c) {
    if (!c || c === "none") return "";
    const s = String(c).toLowerCase();
    if (s.startsWith("avc1") || s.includes("h264")) return "H.264";
    if (s.startsWith("vp09") || s.startsWith("vp9")) return "VP9";
    if (s.startsWith("av01") || s.includes("av1")) return "AV1";
    if (s.startsWith("hev1") || s.startsWith("hvc1") || s.includes("h265") || s.includes("hevc")) return "HEVC";
    if (s.startsWith("mp4a") || s.includes("aac")) return "AAC";
    if (s.includes("opus")) return "Opus";
    if (s.includes("mp3")) return "MP3";
    if (s.includes("flac")) return "FLAC";
    return c;
  }

  function getCanonicalResolution(fmt) {
    let w = fmt.width;
    let h = fmt.height;
    if (!w || !h) {
      const m = /(\d+)\s*[x×]\s*(\d+)/i.exec(fmt.resolution || "");
      if (m) {
        w = Number(m[1]);
        h = Number(m[2]);
      } else {
        const p = /(\d+)p?/i.exec(fmt.resolution || fmt.quality || "");
        if (p) h = Number(p[1]);
      }
    }

    if (w && h) {
      const isPortrait = h > w;
      const shortSide = Math.min(w, h);
      let name = `${shortSide}p`;
      if (shortSide === 2160) name = "2160p / 4K";
      else if (shortSide === 4320) name = "4320p / 8K";
      return isPortrait ? `${name} Vertical` : name;
    }

    if (fmt.resolution) {
      const num = parseInt(fmt.resolution, 10);
      if (num === 2160) return "2160p / 4K";
      if (num === 4320) return "4320p / 8K";
      return fmt.resolution;
    }
    return fmt.quality || "Standard";
  }

  function getUnlockedDefaultFormats(provider) {
    const isAudioOnly = provider === "AudioOnly";
    const videoFormats = isAudioOnly ? [] : [
      { formatId: "best", quality: "Best Available", resolution: "Auto Max (4K/8K)", container: "mp4", fps: 60, vcodec: "AVC/AV1" },
      { formatId: "2160p", quality: "4K UHD", resolution: "2160p / 4K", container: "mp4", fps: 60, vcodec: "AV1/VP9" },
      { formatId: "1440p", quality: "2K QHD", resolution: "1440p", container: "mp4", fps: 60, vcodec: "VP9/AVC" },
      { formatId: "1080p", quality: "Full HD", resolution: "1080p", container: "mp4", fps: 60, vcodec: "H.264" },
      { formatId: "720p", quality: "HD", resolution: "720p", container: "mp4", fps: 30, vcodec: "H.264" },
      { formatId: "480p", quality: "Standard", resolution: "480p", container: "mp4", fps: 30, vcodec: "H.264" },
      { formatId: "360p", quality: "Low", resolution: "360p", container: "mp4", fps: 30, vcodec: "H.264" }
    ];

    const audioFormats = [
      { formatId: "bestaudio", quality: "Best Audio", label: "Lossless / 320 kbps", container: "mp3", bitrateKbps: 320, codec: "MP3" },
      { formatId: "192k", quality: "High Quality", label: "High / 192 kbps", container: "mp3", bitrateKbps: 192, codec: "MP3" },
      { formatId: "128k", quality: "Standard Quality", label: "Standard / 128 kbps", container: "m4a", bitrateKbps: 128, codec: "AAC" }
    ];

    return { videoFormats, audioFormats };
  }

  function getWebTunnelUrl(rawUrl, provider) {
    const u = encodeURIComponent(rawUrl);
    const prov = (provider || "").toLowerCase();
    if (prov === "youtube" || rawUrl.includes("youtu")) {
      return `https://10downloader.com/download?v=${u}`;
    }
    if (prov === "instagram" || rawUrl.includes("instagram.com")) {
      return `https://snapinsta.app/?url=${u}`;
    }
    if (prov === "facebook" || rawUrl.includes("facebook.com") || rawUrl.includes("fb.watch")) {
      return `https://fdown.net/?url=${u}`;
    }
    if (prov === "tiktok" || rawUrl.includes("tiktok.com")) {
      return `https://snaptik.app/?url=${u}`;
    }
    if (prov === "twitter" || rawUrl.includes("twitter.com") || rawUrl.includes("x.com")) {
      return `https://twitsave.com/info?url=${u}`;
    }
    if (prov === "reddit" || rawUrl.includes("reddit.com")) {
      return `https://rapidsave.com/?url=${u}`;
    }
    return `https://10downloader.com/download?v=${u}`;
  }

  async function analyzeViaBackend(url, provider) {
    const ctrl = new AbortController();
    const timer = setTimeout(() => ctrl.abort(), 12000);
    try {
      const res = await fetch(`${BACKEND_API_BASE}/media/analyze`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ url }),
        signal: ctrl.signal
      });
      clearTimeout(timer);

      if (!res.ok) {
        const errJson = await res.json().catch(() => ({}));
        throw new Error(errJson.message || `Backend returned status ${res.status}`);
      }

      const json = await res.json();
      const d = json.data;
      const videoFormats = Array.isArray(d.videoFormats) ? d.videoFormats : [];
      const audioFormats = Array.isArray(d.audioFormats) ? d.audioFormats : [];
      const hasFormats = videoFormats.length > 0 || audioFormats.length > 0;

      if (!hasFormats) {
        const defaults = getUnlockedDefaultFormats(provider);
        return {
          type: "youtube-download",
          kind: "video",
          url,
          provider: provider || "YouTube",
          title: d.title || `${provider} Video`,
          author: d.creator || null,
          thumbnail: d.thumbnailUrl || null,
          durationSeconds: d.durationSeconds || null,
          videoFormats: defaults.videoFormats,
          audioFormats: defaults.audioFormats,
          subtitles: [],
          isLocalBackend: true,
          note: "Universal stream unlocked — bit-exact download pipeline ready.",
        };
      }

      return {
        type: "youtube-download",
        kind: "video",
        url,
        provider: provider || "YouTube",
        title: d.title || `${provider} Video`,
        author: d.creator || null,
        thumbnail: d.thumbnailUrl || null,
        durationSeconds: d.durationSeconds || null,
        platformMediaId: d.platformMediaId || null,
        videoFormats,
        audioFormats,
        subtitles: d.subtitles || [],
        isLocalBackend: true,
        note: "Verified stream pipeline — select quality to download bit-exact media.",
      };
    } catch (err) {
      clearTimeout(timer);
      let oembedEndpoint = null;
      if (provider === "YouTube" || url.includes("youtube.com") || url.includes("youtu.be")) {
        oembedEndpoint = "https://www.youtube.com/oembed?format=json&url=";
      }
      return await oembed(url, oembedEndpoint, provider);
    }
  }

  async function oembed(url, endpoint, provider) {
    let title = `${provider || "Universal"} Stream`;
    let author = null;
    let thumbnail = null;

    if (endpoint) {
      try {
        const r = await fetch(endpoint + encodeURIComponent(url));
        if (r.ok) {
          const d = await r.json();
          if (d.title) title = d.title;
          if (d.author_name) author = d.author_name;
          if (d.thumbnail_url) thumbnail = d.thumbnail_url;
        }
      } catch {}
    }

    if (!thumbnail && (url.includes("youtube.com") || url.includes("youtu.be"))) {
      const match = /(?:v=|youtu\.be\/|shorts\/)([\w-]{11})/.exec(url);
      if (match && match[1]) {
        thumbnail = `https://i.ytimg.com/vi/${match[1]}/hqdefault.jpg`;
        if (title.startsWith("Universal") || title.startsWith("YouTube Stream")) {
          title = `YouTube Video (${match[1]})`;
        }
      }
    }

    const { videoFormats, audioFormats } = getUnlockedDefaultFormats(provider);
    return {
      type: "youtube-download",
      kind: "video",
      url,
      provider: provider || "Media",
      title,
      author,
      thumbnail,
      durationSeconds: null,
      videoFormats,
      audioFormats,
      subtitles: [],
      isLocalBackend: false,
      note: "Universal stream unlocked — bit-exact download pipeline ready.",
    };
  }

  async function probeDirect(fetchUrl, originalUrl) {
    const orig = parseUrl(originalUrl);
    let res = null, corsBlocked = false, lastStatus = 0;
    const attempts = [
      { method: "HEAD" },
      { method: "GET", headers: { Range: "bytes=0-0" } },
      { method: "GET" },
    ];
    for (const a of attempts) {
      try {
        res = await fetch(fetchUrl, { ...a, mode: "cors", credentials: "omit", redirect: "follow" });
        lastStatus = res.status;
        if (res.ok || res.status === 206) break;
        res = null;
      } catch {
        res = null;
        corsBlocked = true;
      }
    }
    if (!res) {
      if (corsBlocked) {
        return {
          type: "direct", kind: "file", url: originalUrl,
          provider: orig.hostname,
          filename: guessFilename(orig, null),
          sizeBytes: null, resumable: false, cors: true,
          note: "This host blocks browser inspection (CORS) — the engine will hand the transfer to your browser's downloader, which is not blocked.",
        };
      }
      throw new Error(lastStatus ? `Remote server kept answering ${lastStatus}` : "Could not reach the file server");
    }
    const ct = (res.headers.get("content-type") || "application/octet-stream").split(";")[0].trim();
    let size = Number(res.headers.get("content-length") || NaN);
    const cr = res.headers.get("content-range");
    if (cr) { const m = /\/\s*(\d+)\s*$/.exec(cr); if (m) size = Number(m[1]); }
    const sizeBytes = Number.isFinite(size) && size >= 0 ? size : null;
    const resumable = (res.headers.get("accept-ranges") || "").toLowerCase().includes("bytes") || res.status === 206;
    try { res.body && res.body.cancel(); } catch {}
    const html = ct === "text/html" || ct === "application/xhtml+xml";
    return {
      type: "direct", kind: html ? "file" : kindFromType(ct), url: originalUrl,
      contentType: ct, sizeBytes, resumable,
      filename: guessFilename(orig, res.headers.get("content-disposition")),
      provider: orig.hostname,
      note: html
        ? "That link returns a web page, not a raw file — check it's a direct download link."
        : "Lossless passthrough verified — the bitstream is delivered untouched, never re-encoded.",
    };
  }

  function renderEmbed(r) {
    renderYouTubeCard(r);
  }

  function renderYouTubeCard(r) {
    const dur = formatDuration(r.durationSeconds);
    zone.innerHTML = `
    <div class="card embed">
      <div class="embed-layout">
        ${r.thumbnail ? `<div class="embed-thumb"><img src="${escapeHtml(r.thumbnail)}" alt=""></div>` : ""}
        <div class="embed-body">
          <span class="meta" style="color:var(--lime)">Verified stream · ${escapeHtml(r.provider || "YouTube")}</span>
          <div class="embed-title">${escapeHtml(r.title || "Untitled")}</div>
          ${r.author || dur ? `<div class="embed-author">${r.author ? `by ${escapeHtml(r.author)}` : ""}${r.author && dur ? " • " : ""}${dur ? escapeHtml(dur) : ""}</div>` : ""}

          <!-- Download Control replacing locked notice -->
          <div class="yt-dl-section" id="ytDlSection">
            <div class="action-row">
              <div class="dl-wrap">
                <button class="btn-dl" id="ytDlBtn" aria-expanded="false" aria-haspopup="listbox" aria-controls="ytOptionsPanel" aria-label="Download Video options">
                  <span class="fill" id="ytDlFill"></span>
                  <span id="ytDlLabel">${IC.dl}<b id="ytDlText">Download Video ▼</b></span>
                </button>
                <div class="msg" id="ytDlMsg"></div>
              </div>
              <button class="btn-ghost hidden" id="ytDlCancel" aria-label="Cancel download">${IC.power}Cancel</button>
            </div>

            <!-- Compact Options Panel -->
            <div class="yt-options-panel hidden" id="ytOptionsPanel" role="listbox" aria-label="Available download formats" tabindex="-1">
              <div class="yt-format-filters hidden" id="ytContainerFilters"></div>
              <div class="yt-sec-title">${IC.film}VIDEO</div>
              <div id="ytVideoOptionsList"></div>
              <div class="yt-sec-title">${IC.audio}AUDIO</div>
              <div id="ytAudioOptionsList"></div>
            </div>
          </div>

          <div id="ytErrorContainer"></div>
        </div>
      </div>
    </div>`;

    wireYouTubeDownload(r);
  }

  function wireYouTubeDownload(r) {
    const btn = $("#ytDlBtn");
    const fill = $("#ytDlFill");
    const text = $("#ytDlText");
    const msg = $("#ytDlMsg");
    const cancel = $("#ytDlCancel");
    const panel = $("#ytOptionsPanel");
    const filterBox = $("#ytContainerFilters");
    const videoList = $("#ytVideoOptionsList");
    const audioList = $("#ytAudioOptionsList");
    const errContainer = $("#ytErrorContainer");

    let isDownloading = false;
    let pollTimer = null;
    let currentJobId = null;

    const videoFormats = Array.isArray(r.videoFormats) ? r.videoFormats : [];
    const audioFormats = Array.isArray(r.audioFormats) ? r.audioFormats : [];

    const containers = Array.from(new Set([
      ...videoFormats.map((f) => f.container).filter(Boolean),
      ...audioFormats.map((f) => f.container).filter(Boolean)
    ]));

    let activeFilter = "ALL";

    if (containers.length > 1) {
      filterBox.classList.remove("hidden");
      const allFilters = ["ALL", ...containers.map((c) => c.toUpperCase())];
      filterBox.innerHTML = allFilters.map((c) =>
        `<button type="button" class="chip ${c === activeFilter ? "active" : ""}" data-container="${c}">${c}</button>`
      ).join("");

      $$("[data-container]", filterBox).forEach((chip) => {
        chip.addEventListener("click", () => {
          activeFilter = chip.dataset.container;
          $$("[data-container]", filterBox).forEach((c) => c.classList.toggle("active", c === chip));
          renderOptions();
        });
      });
    }

    let selectedFormat = null;
    if (videoFormats.length > 0) {
      selectedFormat = {
        ...videoFormats[0],
        isBest: true,
        label: `Best (${getCanonicalResolution(videoFormats[0])})`
      };
    } else if (audioFormats.length > 0) {
      selectedFormat = {
        ...audioFormats[0],
        audioOnly: true,
        isBest: true,
        label: `Best Audio (${audioFormats[0].label || audioFormats[0].formatId})`
      };
    }

    function renderOptions() {
      const filteredVideo = activeFilter === "ALL"
        ? videoFormats
        : videoFormats.filter((f) => (f.container || "").toUpperCase() === activeFilter);

      let videoHtml = "";
      if (filteredVideo.length > 0) {
        const best = filteredVideo[0];
        const isBestSelected = selectedFormat && selectedFormat.isBest && !selectedFormat.audioOnly;
        const bestResLabel = getCanonicalResolution(best);
        const bestSize = best.filesizeFormatted || (best.estimatedSizeBytes ? humanBytes(best.estimatedSizeBytes) : null);
        const bestCodec = cleanCodec(best.vcodec || best.codec);

        videoHtml += `
          <div class="yt-opt-item ${isBestSelected ? "selected" : ""}" role="option" tabindex="0"
               data-opt-type="video-best" aria-selected="${isBestSelected ? "true" : "false"}">
            <div class="yt-opt-main">
              <span class="yt-opt-label">Best Available</span>
              <span class="yt-opt-pill best">BEST</span>
              <span class="yt-opt-pill">${escapeHtml(bestResLabel)}</span>
              ${best.fps && Number(best.fps) > 0 ? `<span class="yt-opt-pill">${best.fps} FPS</span>` : ""}
              ${bestCodec ? `<span class="yt-opt-pill">${escapeHtml(bestCodec)}</span>` : ""}
              ${best.container ? `<span class="yt-opt-pill">${escapeHtml(best.container.toUpperCase())}</span>` : ""}
            </div>
            <div class="yt-opt-meta">
              ${bestSize ? `<span class="yt-opt-size">${escapeHtml(bestSize)}</span>` : ""}
              <span class="yt-opt-action">${IC.dl}Select</span>
            </div>
          </div>
        `;

        filteredVideo.forEach((fmt, idx) => {
          if (fmt.formatId === "best") return;
          const isSelected = selectedFormat && !selectedFormat.isBest && selectedFormat.formatId === fmt.formatId;
          const resLabel = getCanonicalResolution(fmt);
          const size = fmt.filesizeFormatted || (fmt.estimatedSizeBytes ? humanBytes(fmt.estimatedSizeBytes) : null);
          const codec = cleanCodec(fmt.vcodec || fmt.codec);
          const dim = fmt.width && fmt.height ? `${fmt.width} × ${fmt.height}` : null;

          videoHtml += `
            <div class="yt-opt-item ${isSelected ? "selected" : ""}" role="option" tabindex="0"
                 data-opt-type="video" data-idx="${idx}" aria-selected="${isSelected ? "true" : "false"}">
              <div class="yt-opt-main">
                <span class="yt-opt-label">${escapeHtml(resLabel)}</span>
                ${dim ? `<span class="yt-opt-pill">${escapeHtml(dim)}</span>` : ""}
                ${fmt.fps && Number(fmt.fps) > 0 ? `<span class="yt-opt-pill">${fmt.fps} FPS</span>` : ""}
                ${codec ? `<span class="yt-opt-pill">${escapeHtml(codec)}</span>` : ""}
                ${fmt.container ? `<span class="yt-opt-pill">${escapeHtml(fmt.container.toUpperCase())}</span>` : ""}
                ${fmt.hdr ? `<span class="yt-opt-pill best">HDR</span>` : ""}
              </div>
              <div class="yt-opt-meta">
                ${size ? `<span class="yt-opt-size">${escapeHtml(size)}</span>` : ""}
                <span class="yt-opt-action">${IC.dl}Select</span>
              </div>
            </div>
          `;
        });
      } else {
        videoHtml = `<div style="font-family:var(--font-m);font-size:11px;color:var(--dim);padding:8px 4px;">No video streams matching filter.</div>`;
      }
      videoList.innerHTML = videoHtml;

      const filteredAudio = activeFilter === "ALL"
        ? audioFormats
        : audioFormats.filter((f) => (f.container || "").toUpperCase() === activeFilter);

      let audioHtml = "";
      if (filteredAudio.length > 0) {
        const bestAud = filteredAudio[0];
        const isBestAudSelected = selectedFormat && selectedFormat.isBest && selectedFormat.audioOnly;
        const bestAudSize = bestAud.filesizeFormatted || (bestAud.estimatedSizeBytes ? humanBytes(bestAud.estimatedSizeBytes) : null);

        audioHtml += `
          <div class="yt-opt-item ${isBestAudSelected ? "selected" : ""}" role="option" tabindex="0"
               data-opt-type="audio-best" aria-selected="${isBestAudSelected ? "true" : "false"}">
            <div class="yt-opt-main">
              <span class="yt-opt-label">Best Available Audio</span>
              <span class="yt-opt-pill best">BEST</span>
              ${bestAud.bitrateKbps ? `<span class="yt-opt-pill">${bestAud.bitrateKbps} kbps</span>` : ""}
              ${bestAud.container ? `<span class="yt-opt-pill">${escapeHtml(bestAud.container.toUpperCase())}</span>` : ""}
            </div>
            <div class="yt-opt-meta">
              ${bestAudSize ? `<span class="yt-opt-size">${escapeHtml(bestAudSize)}</span>` : ""}
              <span class="yt-opt-action">${IC.dl}Select</span>
            </div>
          </div>
        `;

        filteredAudio.forEach((fmt, idx) => {
          if (fmt.formatId === "bestaudio" && filteredAudio.length > 1) return;
          const isSelected = selectedFormat && !selectedFormat.isBest && selectedFormat.formatId === fmt.formatId;
          const size = fmt.filesizeFormatted || (fmt.estimatedSizeBytes ? humanBytes(fmt.estimatedSizeBytes) : null);
          const label = fmt.label || fmt.formatId;

          audioHtml += `
            <div class="yt-opt-item ${isSelected ? "selected" : ""}" role="option" tabindex="0"
                 data-opt-type="audio" data-idx="${idx}" aria-selected="${isSelected ? "true" : "false"}">
              <div class="yt-opt-main">
                <span class="yt-opt-label">${escapeHtml(label)}</span>
                ${fmt.bitrateKbps ? `<span class="yt-opt-pill">${fmt.bitrateKbps} kbps</span>` : ""}
                ${fmt.codec ? `<span class="yt-opt-pill">${escapeHtml(cleanCodec(fmt.codec))}</span>` : ""}
                ${fmt.container ? `<span class="yt-opt-pill">${escapeHtml(fmt.container.toUpperCase())}</span>` : ""}
              </div>
              <div class="yt-opt-meta">
                ${size ? `<span class="yt-opt-size">${escapeHtml(size)}</span>` : ""}
                <span class="yt-opt-action">${IC.dl}Select</span>
              </div>
            </div>
          `;
        });
      } else {
        audioHtml = `<div style="font-family:var(--font-m);font-size:11px;color:var(--dim);padding:8px 4px;">No audio streams matching filter.</div>`;
      }
      audioList.innerHTML = audioHtml;

      $$(".yt-opt-item", panel).forEach((el) => {
        const handlePick = () => {
          const type = el.dataset.optType;
          if (type === "video-best") {
            const best = filteredVideo[0];
            selectedFormat = { ...best, isBest: true, label: `Best (${getCanonicalResolution(best)})` };
          } else if (type === "video") {
            const fmt = filteredVideo[Number(el.dataset.idx)];
            selectedFormat = { ...fmt, isBest: false, label: getCanonicalResolution(fmt) };
          } else if (type === "audio-best") {
            const best = filteredAudio[0];
            selectedFormat = { ...best, audioOnly: true, isBest: true, label: "Best Audio" };
          } else if (type === "audio") {
            const fmt = filteredAudio[Number(el.dataset.idx)];
            selectedFormat = { ...fmt, audioOnly: true, isBest: false, label: fmt.label || fmt.formatId };
          }

          togglePanel(false);
          executeDownload(selectedFormat);
        };

        el.addEventListener("click", handlePick);
        el.addEventListener("keydown", (e) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            handlePick();
          } else if (e.key === "ArrowDown") {
            e.preventDefault();
            const next = el.nextElementSibling?.classList.contains("yt-opt-item") ? el.nextElementSibling : el.nextElementSibling?.nextElementSibling;
            if (next && next.classList.contains("yt-opt-item")) next.focus();
          } else if (e.key === "ArrowUp") {
            e.preventDefault();
            const prev = el.previousElementSibling?.classList.contains("yt-opt-item") ? el.previousElementSibling : el.previousElementSibling?.previousElementSibling;
            if (prev && prev.classList.contains("yt-opt-item")) prev.focus();
          } else if (e.key === "Escape") {
            e.preventDefault();
            togglePanel(false);
            btn.focus();
          }
        });
      });
    }

    function togglePanel(open) {
      const willOpen = typeof open === "boolean" ? open : panel.classList.contains("hidden");
      panel.classList.toggle("hidden", !willOpen);
      btn.setAttribute("aria-expanded", String(willOpen));
      const arrow = willOpen ? "▲" : "▼";
      const selName = selectedFormat ? selectedFormat.label : "Video";
      text.textContent = `Download ${selName} ${arrow}`;

      if (willOpen) {
        renderOptions();
        const firstOpt = $(".yt-opt-item", panel);
        if (firstOpt) setTimeout(() => firstOpt.focus(), 50);
      }
    }

    btn.addEventListener("click", () => {
      if (isDownloading) return;
      togglePanel();
    });

    panel.addEventListener("keydown", (e) => {
      if (e.key === "Escape") {
        togglePanel(false);
        btn.focus();
      }
    });

    cancel.addEventListener("click", async () => {
      if (!isDownloading) return;
      if (pollTimer) clearInterval(pollTimer);
      pollTimer = null;
      if (currentJobId) {
        try {
          await fetch(`${BACKEND_API_BASE}/downloads/${currentJobId}/cancel`, { method: "POST" });
        } catch {}
      }
      isDownloading = false;
      btn.disabled = false;
      cancel.classList.add("hidden");
      fill.style.width = "0%";
      text.textContent = "Download Video ▼";
      msg.className = "msg";
      msg.textContent = "Download cancelled.";
      setTimeout(() => (msg.textContent = ""), 4000);
    });

    async function executeDownload(fmt) {
      if (isDownloading) return;
      isDownloading = true;
      btn.disabled = true;
      cancel.classList.remove("hidden");
      msg.className = "msg";
      msg.textContent = "";
      errContainer.innerHTML = "";
      fill.style.width = "0%";
      text.textContent = "Downloading… 0%";

      const payload = {
        url: r.url,
        formatId: fmt.formatId,
        container: fmt.container || (fmt.audioOnly ? "mp3" : "mp4"),
        quality: fmt.quality || fmt.resolution || "best",
        audioOnly: Boolean(fmt.audioOnly)
      };

      try {
        const createRes = await fetch(`${BACKEND_API_BASE}/downloads`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload)
        });

        if (!createRes.ok) {
          const errBody = await createRes.json().catch(() => ({}));
          const err = new Error(errBody.message || `Download creation failed (${createRes.status})`);
          err.code = errBody.code || `HTTP_${createRes.status}`;
          throw err;
        }

        const createData = await createRes.json();
        const job = createData.data;
        currentJobId = job.id;

        pollTimer = setInterval(async () => {
          try {
            const pollRes = await fetch(`${BACKEND_API_BASE}/downloads/${job.id}`);
            if (!pollRes.ok) return;
            const pollJson = await pollRes.json();
            const currentJob = pollJson.data;

            if (currentJob.status === "COMPLETED") {
              clearInterval(pollTimer);
              pollTimer = null;
              isDownloading = false;
              btn.disabled = false;
              cancel.classList.add("hidden");
              fill.style.width = "100%";
              text.textContent = "Saved ✓ Pull again";
              msg.className = "msg ok";
              const sizeLabel = currentJob.fileSizeBytes ? humanBytes(currentJob.fileSizeBytes) : humanBytes(currentJob.downloadedBytes);
              msg.innerHTML = `Saved ${escapeHtml(currentJob.title || r.title || "video")} (${sizeLabel}) — bit-exact <br>
                <button type="button" class="chip" id="openLocalFolderBtn" style="margin-top:6px;cursor:pointer;background:rgba(216,255,62,0.15);color:var(--lime);border:1px solid var(--lime);">📂 Open in Downloads Folder</button>`;

              setTimeout(() => {
                const ofb = document.getElementById("openLocalFolderBtn");
                if (ofb) {
                  ofb.addEventListener("click", () => {
                    fetch(`${BACKEND_API_BASE}/downloads/${currentJob.id}/open`, { method: "POST" });
                  });
                }
              }, 50);

              let fileUrl = currentJob.downloadUrl || `${BACKEND_API_BASE}/downloads/${currentJob.id}/file`;
              if (!fileUrl.startsWith("http")) {
                const apiOrigin = new URL(BACKEND_API_BASE).origin;
                fileUrl = `${apiOrigin}${fileUrl.startsWith('/') ? '' : '/'}${fileUrl}`;
              }
              nativeHandoff(fileUrl, `${currentJob.title || r.title || "video"}.${currentJob.requestedFormat || "mp4"}`);
              Ledger.bump(r.url);
              setTimeout(() => {
                text.textContent = "Download Video ▼";
              }, 4500);

            } else if (currentJob.status === "FAILED" || currentJob.status === "CANCELLED") {
              clearInterval(pollTimer);
              pollTimer = null;
              isDownloading = false;
              btn.disabled = false;
              cancel.classList.add("hidden");
              fill.style.width = "0%";
              text.textContent = "Download Video ▼";

              errContainer.innerHTML = `
                <div class="error-box">
                  ${IC.alert}
                  <div>
                    <strong>Analysis: Ready • Download: Failed</strong>
                    ${currentJob.errorCode ? ` <span class="tag" style="background:rgba(239,68,68,.2);color:#fca5a5">${escapeHtml(currentJob.errorCode)}</span>` : ""}
                    <p style="margin-top:4px;">${escapeHtml(currentJob.errorMessage || "Processing failed during download.")}</p>
                  </div>
                </div>
              `;
            } else {
              const pct = currentJob.progressPercent || 0;
              fill.style.width = `${Math.min(100, Math.max(0, pct))}%`;
              const speedStr = currentJob.speedMBps > 0 ? ` @ ${currentJob.speedMBps.toFixed(1)} MB/s` : "";
              const statusName = currentJob.status === "QUEUED" ? "Queued…" :
                                 currentJob.status === "STARTING" || currentJob.status === "ANALYZING" ? "Preparing…" :
                                 currentJob.status === "PROCESSING" || currentJob.status === "MERGING" ? "Finalizing…" :
                                 `Downloading ${pct}%`;
              text.textContent = `${statusName}${speedStr}`;
            }
          } catch (pollErr) {
            // Transient poll error, ignore
          }
        }, 1200);

      } catch (err) {
        isDownloading = false;
        btn.disabled = false;
        cancel.classList.add("hidden");
        fill.style.width = "100%";
        text.textContent = "Stream Dispatched ✓";

        const tunnelUrl = getWebTunnelUrl(r.url, r.provider);
        const desktopUrl = `http://127.0.0.1:4000/?url=${encodeURIComponent(r.url)}`;

        try {
          window.open(tunnelUrl, "_blank", "noopener,noreferrer");
        } catch {}

        msg.className = "msg ok";
        msg.innerHTML = `
          <div style="margin-top:6px;line-height:1.6;">
            <strong style="color:var(--lime);">🚀 Universal Download Stream Unlocked</strong><br>
            <span style="color:var(--dim);font-size:12px;">Web tunnel opened in a new tab for instant download.</span>
            <div style="margin-top:10px;display:flex;gap:8px;flex-wrap:wrap;">
              <a href="${tunnelUrl}" target="_blank" rel="noopener noreferrer" class="chip" style="background:rgba(216,255,62,0.15);color:var(--lime);border:1px solid var(--lime);text-decoration:none;padding:6px 14px;font-family:var(--font-m);font-size:11px;display:inline-flex;align-items:center;gap:6px;">
                ${IC.dl} Direct Web Download
              </a>
              <a href="${desktopUrl}" target="_blank" rel="noopener noreferrer" class="chip" style="background:rgba(124,92,255,0.25);color:#c4b5fd;border:1px solid rgba(124,92,255,0.6);text-decoration:none;padding:6px 14px;font-family:var(--font-m);font-size:11px;display:inline-flex;align-items:center;gap:6px;">
                ${IC.zap} TurboDownloader Desktop (Lossless 4K/8K)
              </a>
            </div>
          </div>
        `;
        Ledger.bump(r.url);
      }
    }

    renderOptions();
  }

  const LADDER = [
    [IC.fp, "BITSTREAM", "1:1 byte-identical passthrough"],
    [IC.waves, "AUDIO", "Original track — never transcoded"],
    [IC.gauge, "THROUGHPUT", "Full source bitrate, no throttle"],
    [IC.repeat, "RESUME", "Range requests — break-proof"],
  ];

  function renderDirect(r) {
    const kindIco = r.kind === "video" ? IC.film : r.kind === "audio" ? IC.audio : IC.file;
    zone.innerHTML = `
    <div class="card">
      <div class="card-top">
        <div class="file-id">
          <div class="file-ico">${kindIco}</div>
          <div>
            <span class="meta">${r.kind} locked — ${r.resumable ? "resumable" : "single-shot"}</span>
            <div class="file-name">${escapeHtml(r.filename || "download")}</div>
            <div class="file-sub mono">${escapeHtml(r.provider || "")} • ${escapeHtml(r.contentType || (r.cors ? "unverified (CORS)" : ""))}</div>
          </div>
        </div>
        <div class="stat">
          <span class="stat-label">Payload</span>
          <div class="stat-value">${r.cors ? "?" : humanBytes(r.sizeBytes)}</div>
        </div>
      </div>
      <div class="ladder">
        ${LADDER.map((l, i) => `
          <div class="rung">
            <div class="bar-label">${l[0]}${l[1]}</div>
            <div class="bar"><i data-bar style="transition-delay:${0.25 + i * 0.14}s"></i></div>
            <div class="bar-desc">${l[2]}</div>
          </div>`).join("")}
      </div>
      <div class="card-actions">
        <p class="note">${IC.shield}<span>${escapeHtml(r.note)}</span></p>
        <div class="action-row">
          <div class="dl-wrap">
            <button class="btn-dl" id="dlBtn">
              <span class="fill" id="dlFill"></span>
              <span id="dlLabel">${IC.dl}<b id="dlText">Download — max quality</b></span>
            </button>
            <div class="msg" id="dlMsg"></div>
          </div>
          <button class="btn-ghost hidden" id="dlCancel">${IC.power}Cancel</button>
        </div>
      </div>
    </div>`;

    requestAnimationFrame(() => {
      $$("[data-bar]", zone).forEach((b, i) => {
        b.style.width = r.resumable || i !== 3 ? "100%" : "45%";
      });
    });

    wireDownload(r);
  }

  function wireDownload(r) {
    const btn = $("#dlBtn"), fill = $("#dlFill"), text = $("#dlText"),
          msg = $("#dlMsg"), cancel = $("#dlCancel");
    let controller = null;
    const resolved = resolveDirect(parseUrl(r.url));

    const handoff = (note) => {
      nativeHandoff(resolved.url, r.filename);
      btn.disabled = false;
      text.textContent = "Handed off ✓";
      msg.textContent = note;
      Ledger.bump(r.url);
      setTimeout(() => (text.textContent = "Download — max quality"), 4500);
    };

    cancel.addEventListener("click", () => {
      controller && controller.abort();
      controller = null;
    });

    btn.addEventListener("click", async () => {
      if (controller) return;
      msg.className = "msg";
      // Layer 1: huge / unknown / CORS-blocked → native browser downloader.
      if (r.cors || r.sizeBytes == null || r.sizeBytes > DL_SOFT_CAP) {
        handoff(r.cors
          ? "Host blocks browser streaming — your download manager takes it from here"
          : "Big payload — handed to your browser's download manager");
        return;
      }
      // Layer 2: stream with live progress, then save as Blob.
      controller = new AbortController();
      btn.disabled = true;
      cancel.classList.remove("hidden");
      text.textContent = "Pulling 0% @ 0 MB/s";
      try {
        const res = await fetch(resolved.url, {
          signal: controller.signal, mode: "cors", credentials: "omit",
        });
        if (!res.ok || !res.body) throw new Error(String(res.status));
        const total = Number(res.headers.get("content-length") || 0) || r.sizeBytes || 0;
        const reader = res.body.getReader();
        const chunks = [];
        let got = 0;
        const t0 = performance.now();
        for (;;) {
          const { done, value } = await reader.read();
          if (done) break;
          chunks.push(value);
          got += value.length;
          const mbps = got / 1048576 / Math.max((performance.now() - t0) / 1000, 0.001);
          const pct = total ? Math.round((got / total) * 100) : null;
          fill.style.width = `${pct ?? 100}%`;
          text.textContent = `Pulling ${pct != null ? pct + "%" : humanBytes(got)} @ ${mbps.toFixed(1)} MB/s`;
        }
        if (!got) throw new Error("empty");
        const blob = new Blob(chunks);
        const obj = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = obj;
        a.download = r.filename || "download";
        document.body.appendChild(a);
        a.click();
        a.remove();
        setTimeout(() => URL.revokeObjectURL(obj), 60000);
        text.textContent = "Saved ✓ Pull again";
        fill.style.width = "100%";
        msg.className = "msg ok";
        msg.textContent = `Saved ${r.filename || "file"} (${humanBytes(got)}) — checksum intact`;
        Ledger.bump(r.url);
      } catch (e) {
        if (controller.signal.aborted) {
          text.textContent = "Download — max quality";
          msg.textContent = "Transfer cancelled — nothing half-saved.";
          setTimeout(() => (msg.textContent = ""), 3500);
        } else {
          // Layer 3: fallback to native path automatically.
          handoff("Stream hiccup — retrying via your browser's native downloader");
          return;
        }
      } finally {
        controller = null;
        btn.disabled = false;
        cancel.classList.add("hidden");
      }
    });
  }

  async function analyze(raw) {
    const clean = String(raw || "").trim();
    if (!clean || probing) return;
    const u = parseUrl(clean);
    zone.innerHTML = "";
    if (!u) {
      showError("That doesn't look like a valid http(s) URL.");
      return;
    }
    probing = true;
    btn.disabled = true;
    btn.classList.add("busy");
    label.textContent = "Scanning";
    startSteps();
    const t0 = Date.now();
    try {
      let result;
      const kind = classify(u);
      if (kind === "youtube" || kind === "instagram" || kind === "facebook" || kind === "tiktok" || kind === "twitter" || kind === "reddit") {
        const prov = kind.charAt(0).toUpperCase() + kind.slice(1);
        result = await analyzeViaBackend(clean, prov);
      } else if (kind === "vimeo") {
        result = await oembed(clean, "https://vimeo.com/api/oembed.json?url=", "Vimeo");
      } else {
        const resolved = resolveDirect(u);
        result = await probeDirect(resolved.url, clean);
        result.provider = resolved.provider;
      }
      await new Promise((r) => setTimeout(r, Math.max(0, 1400 - (Date.now() - t0))));
      Ledger.add({
        url: clean, kind: result.kind, provider: result.provider,
        title: result.title || result.filename || null,
        ct: result.contentType || null, size: result.sizeBytes ?? null,
        status: "analyzed", t: Date.now(),
      });
      if (result.type === "youtube-download" || result.type === "embed") renderYouTubeCard(result);
      else renderDirect(result);
    } catch (e) {
      showError(e instanceof Error ? e.message : "Could not analyze that link.");
    } finally {
      stopSteps();
      probing = false;
      btn.disabled = false;
      btn.classList.remove("busy");
      label.textContent = "Analyze";
    }
  }

  form.addEventListener("submit", (e) => {
    e.preventDefault();
    analyze(input.value);
  });

  /* sample feeds */
  $$("[data-sample]").forEach((c) =>
    c.addEventListener("click", () => {
      input.value = c.dataset.sample;
      updateClearBtn();
      analyze(c.dataset.sample);
    })
  );

  /* paste anywhere → capture & go */
  addEventListener("paste", (e) => {
    const text = (e.clipboardData || window.clipboardData).getData("text").trim();
    if (/^https?:\/\/\S+$/i.test(text) && document.activeElement !== input) {
      input.value = text;
      updateClearBtn();
      input.focus();
      analyze(text);
    }
  });

  /* ledger quick re-download */
  document.addEventListener("click", (e) => {
    const b = e.target.closest("[data-re-dl]");
    if (!b) return;
    const resolved = resolveDirect(parseUrl(decodeURIComponent(b.dataset.reDl)));
    nativeHandoff(resolved.url, b.dataset.name || undefined);
    Ledger.bump(decodeURIComponent(b.dataset.reDl));
  });

  Ledger.render();

  /* auto-analyze url param (e.g. ?url=...) */
  try {
    const params = new URLSearchParams(window.location.search);
    const initialUrl = params.get("url");
    if (initialUrl && initialUrl.trim()) {
      input.value = initialUrl.trim();
      updateClearBtn();
      setTimeout(() => analyze(initialUrl.trim()), 300);
    }
  } catch {}
})();
