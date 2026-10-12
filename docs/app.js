/* ============ PEAK/8K — static edition engine ============ */
"use strict";

/* ============ Security Hardening & Tamper Defense ============ */
(function sealRuntimeSecurity() {
  try {
    if (typeof window !== "undefined" && window.location.hostname !== "localhost" && window.location.hostname !== "127.0.0.1") {
      const noop = function() {};
      console.log = noop;
      console.debug = noop;
      console.info = noop;
      console.table = noop;
      console.dir = noop;
    }
    if (typeof console !== "undefined" && console.warn) {
      console.warn(
        "%c[SECURITY SHIELD ACTIVE]\n180hz Bit-Exact Download Engine.\nConsole snooping & memory inspection restricted.",
        "color:#d8ff3e;background:#12131a;font-size:12px;font-weight:bold;padding:4px 8px;border-radius:4px;"
      );
    }
  } catch (e) {}
})();

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
    const clearBtn = $("#clearLedgerBtn");
    if (clearBtn) {
      clearBtn.style.display = items.length ? "inline-flex" : "none";
    }
    if (!items.length) {
      body.innerHTML = `<div class="empty">${IC.search}Ledger empty — feed the engine its first link</div>`;
      return;
    }
    const icons = { video: IC.film, audio: IC.audio, file: IC.file, embed: IC.globe };
    const extLinkIco = `<svg viewBox="0 0 24 24" class="ico" style="width:13px;height:13px;fill:none;stroke:currentColor;stroke-width:2;stroke-linecap:round;stroke-linejoin:round;"><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/><polyline points="15 3 21 3 21 9"/><line x1="10" y1="14" x2="21" y2="3"/></svg>`;

    body.innerHTML = items.map((j, i) => `
      <div class="row-item" style="animation-delay:${i * 0.05}s">
        <div class="row-ico">${icons[j.kind] || IC.file}</div>
        <div class="row-body">
          <div class="row-title">${escapeHtml(j.title || j.provider || j.url)}</div>
          <div class="row-sub">${escapeHtml(j.provider || "")}${j.ct ? " • " + escapeHtml(j.ct) : ""}</div>
        </div>
        <div class="row-size">${humanBytes(j.size)}</div>
        <div class="row-actions">
          ${j.kind !== "embed" ? `<button type="button" class="row-action-btn row-dl" data-re-dl="${encodeURIComponent(j.url)}" data-name="${escapeHtml(j.title || "")}" title="Analyze & Direct Download">${IC.dl}</button>` : ""}
          <a href="${escapeHtml(j.url)}" target="_blank" rel="noopener noreferrer" class="row-action-btn row-link" data-open-external="${encodeURIComponent(j.url)}" title="Open original video link in browser">${extLinkIco}</a>
        </div>
        <span class="tag ${j.status === "downloaded" ? "done" : ""}">${j.status}</span>
        <span class="row-time">${relTime(j.t)}</span>
      </div>`).join("");
  },
  clear() {
    try {
      localStorage.removeItem(LKEY);
    } catch {}
    Ledger.render();
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

async function triggerSaveAs(url, filename) {
  const safeFilename = filename || "download.mp4";
  if (typeof window !== "undefined" && "showSaveFilePicker" in window) {
    try {
      const ext = safeFilename.split('.').pop().toLowerCase() || 'mp4';
      const isAudio = ext === 'mp3' || ext === 'm4a' || ext === 'wav' || ext === 'opus';
      const mimeType = isAudio ? `audio/${ext}` : `video/${ext}`;
      const handle = await window.showSaveFilePicker({
        suggestedName: safeFilename,
        types: [{
          description: isAudio ? 'Audio File' : 'Video File',
          accept: { [mimeType]: [`.${ext}`] }
        }]
      });
      const writable = await handle.createWritable();
      const res = await fetch(url);
      if (!res.ok) throw new Error("HTTP " + res.status);
      await res.body.pipeTo(writable);
      return;
    } catch (err) {
      if (err.name === 'AbortError') return; // User cancelled picker
    }
  }

  // Blob URL fallback (prevents mixed-content blocking on HTTPS)
  try {
    const res = await fetch(url);
    if (!res.ok) throw new Error("HTTP " + res.status);
    const blob = await res.blob();
    const blobUrl = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = blobUrl;
    a.download = safeFilename;
    document.body.appendChild(a);
    a.click();
    setTimeout(() => {
      a.remove();
      URL.revokeObjectURL(blobUrl);
    }, 15000);
    return;
  } catch (err) {
    nativeHandoff(url, safeFilename);
  }
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

  function updateInputUi() {
    const val = input && input.value ? input.value.trim() : "";
    if (clearBtn) {
      if (val.length > 0) clearBtn.classList.remove("hidden");
      else clearBtn.classList.add("hidden");
    }
    if (btn) {
      if (val.length > 0) btn.classList.add("ready");
      else btn.classList.remove("ready");
    }
  }

  function highlightAnalyzeReady() {
    updateInputUi();
    if (btn) {
      btn.classList.add("pulse-ready");
      setTimeout(() => btn.classList.remove("pulse-ready"), 1600);
    }
  }

  if (clearBtn) {
    clearBtn.addEventListener("click", () => {
      input.value = "";
      updateInputUi();
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
            updateInputUi();
            input.focus();
            highlightAnalyzeReady();
            return;
          }
        }
      } catch {}
      input.focus();
      input.select();
    });
  }

  input.addEventListener("input", updateInputUi);
  input.addEventListener("change", updateInputUi);
  input.addEventListener("keyup", updateInputUi);

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
  function buildErrorCardHtml(opts = {}) {
    const title = opts.title || "Download Failed";
    let cause = String(opts.cause || opts.errorMessage || opts.message || "").trim();
    if (!cause) {
      cause = "The media stream could not be fetched or processed from the host platform.";
    }
    const code = opts.code || null;
    const retryId = opts.retryId || null;
    const retryText = opts.retryText || "🔄 Retry Download";
    const tunnelUrl = opts.tunnelUrl || null;
    const rawUrl = opts.url || (input && input.value ? input.value.trim() : "");
    const showAppDownload = opts.showAppDownload !== false;

    // Contextual English explanation
    let detailedExplanation = "";
    const lower = cause.toLowerCase();
    if (lower.includes("login") || lower.includes("cookie") || lower.includes("bot") || lower.includes("empty media response") || lower.includes("restricted")) {
      detailedExplanation = "This video or audio requires platform authentication, age verification, or is restricted by host security.";
    } else if (lower.includes("private") || lower.includes("unavailable") || lower.includes("deleted") || lower.includes("not found") || lower.includes("404")) {
      detailedExplanation = "The media file is private, deleted, or has been removed from the host platform.";
    } else if (lower.includes("connect") || lower.includes("failed to fetch") || lower.includes("networkerror") || lower.includes("local engine") || lower.includes("4000")) {
      detailedExplanation = "Cannot connect to the 180Hz Engine or the connection was blocked by browser network rules.";
    } else if (lower.includes("valid http")) {
      detailedExplanation = "The provided link is invalid or is not from a supported platform.";
    } else {
      detailedExplanation = "An unexpected network or platform limitation occurred while retrieving this stream.";
    }

    const currentMediaUrl = rawUrl || (typeof window !== "undefined" ? window.location.href : "");
    const issueTitle = encodeURIComponent(`[Issue]: ${title}`);
    const issueBody = encodeURIComponent(
      `### Issue Report\n` +
      `- **Error Title:** ${title}\n` +
      `- **Cause:** ${cause}\n` +
      `- **Media URL:** ${currentMediaUrl || "N/A"}\n` +
      `- **Environment:** ${isDesktopEnvironment() ? "180Hz Desktop Engine" : "Web Portal"}\n\n` +
      `*Reported automatically from 180Hz Downloader Web UI.*`
    );
    const emailSubject = encodeURIComponent(`180Hz Downloader Issue Report: ${title}`);
    const emailBody = encodeURIComponent(
      `Hello Tanjeem,\n\nI encountered the following download error on 180Hz Downloader:\n\n` +
      `Title: ${title}\n` +
      `Cause: ${cause}\n` +
      `URL: ${currentMediaUrl}\n\n` +
      `Please check and resolve this.\nThank you!`
    );

    return `
      <div class="error-box error-box-enhanced">
        <div class="error-box-icon">
          ${IC.alert}
        </div>
        <div class="error-box-body">
          <div class="error-box-header">
            <div class="error-title-wrap">
              <strong class="error-title">${escapeHtml(title)}</strong>
              ${code ? `<span class="tag error-tag">${escapeHtml(code)}</span>` : ""}
            </div>
            <span class="error-alert-badge">Notice</span>
          </div>

          <!-- 1. Cause Section (FIRST as requested) -->
          <div class="error-cause-card">
            <div class="error-cause-label">
              <svg viewBox="0 0 24 24" class="ico" style="width:13px;height:13px;fill:none;stroke:currentColor;stroke-width:2;"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              <span>Error Cause:</span>
            </div>
            <p class="error-cause-text">
              <strong>${escapeHtml(cause)}</strong>
            </p>
            ${detailedExplanation ? `<p style="font-size:12px;line-height:1.5;color:#fca5a5;margin-top:6px;">ℹ️ ${escapeHtml(detailedExplanation)}</p>` : ""}
          </div>

          <!-- 2. Contact Tanjeem (Owner) Section -->
          <div class="owner-contact-card">
            <div class="owner-contact-header">
              <div class="owner-badge-wrap">
                <span class="owner-avatar-icon">👑</span>
                <span class="owner-contact-title">
                  Need Help or Fix? Contact <strong>Tanjeem (Owner)</strong>:
                </span>
              </div>
              <span class="owner-subtext">Direct Developer Support</span>
            </div>
            <p class="owner-contact-desc">
              If this link fails to download or you encounter issues, please contact <strong>Tanjeem (Owner)</strong> directly:
            </p>
            <div class="owner-contact-actions">
              <a href="https://github.com/tanjeem180hz" target="_blank" rel="noopener noreferrer" class="contact-btn contact-btn-github" title="Visit Tanjeem's GitHub Profile">
                <svg viewBox="0 0 24 24" class="ico" style="width:13px;height:13px;fill:currentColor;"><path d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0024 12c0-6.63-5.37-12-12-12z"/></svg>
                <span>GitHub: @tanjeem180hz</span>
              </a>
              <a href="https://github.com/tanjeem180hz/180hz_video-downloader/issues/new?title=${issueTitle}&body=${issueBody}" target="_blank" rel="noopener noreferrer" class="contact-btn contact-btn-issue" title="Report issue directly on GitHub">
                <svg viewBox="0 0 24 24" class="ico" style="width:13px;height:13px;fill:none;stroke:currentColor;stroke-width:2;"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                <span>🐛 Report Issue</span>
              </a>
              <a href="mailto:pbpb4709@gmail.com?subject=${emailSubject}&body=${emailBody}" class="contact-btn contact-btn-mail" title="Email Owner directly">
                <svg viewBox="0 0 24 24" class="ico" style="width:13px;height:13px;fill:none;stroke:currentColor;stroke-width:2;"><rect x="2" y="4" width="20" height="16" rx="2"/><path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"/></svg>
                <span>✉️ Email Owner</span>
              </a>
            </div>
          </div>

          <!-- 3. Immediate Action Buttons -->
          <div class="error-actions-row">
            ${tunnelUrl ? `
              <a href="${tunnelUrl}" target="_blank" rel="noopener" class="post-dl-btn post-dl-btn-primary" style="text-decoration:none;">
                ⚡ Direct Download via Web Mirror
              </a>
            ` : ""}
            ${retryId ? `
              <button type="button" class="post-dl-btn post-dl-btn-redownload" id="${retryId}">
                ${escapeHtml(retryText)}
              </button>
            ` : ""}
            ${(showAppDownload && !isDesktopEnvironment()) ? `
              <a href="180Hz Downloader Setup.exe" download="180Hz Downloader Setup.exe" class="post-dl-btn" style="background:rgba(216,255,62,0.12);color:var(--lime);border:1px solid rgba(216,255,62,0.3);text-decoration:none;">
                💻 Try in 180Hz Downloader (.exe)
              </a>
            ` : ""}
          </div>
        </div>
      </div>
    `;
  }

  function showError(msg, details = {}) {
    const causeText = typeof msg === "string" ? msg : (msg && msg.message ? msg.message : "Could not analyze link.");
    const retryId = "errAnalyzeRetryBtn";
    zone.innerHTML = buildErrorCardHtml({
      title: details.title || "Link Analysis Failed",
      cause: causeText,
      code: details.code || "ANALYZE_ERROR",
      retryId,
      retryText: "🔄 Try Again",
      url: details.url || (input && input.value ? input.value.trim() : ""),
      showAppDownload: true
    });
    setTimeout(() => {
      const rb = document.getElementById(retryId);
      if (rb) {
        rb.addEventListener("click", () => {
          if (details.onRetry) details.onRetry();
          else if (input && input.value) analyze(input.value);
        });
      }
    }, 50);
  }

  function isDesktopEnvironment() {
    if (typeof window === "undefined") return false;
    const port = String(window.location.port || "");
    const host = String(window.location.hostname || "").toLowerCase();
    if (port === "4000" || host === "127.0.0.1" || host === "localhost") return true;
    if (window.DESKTOP_CONNECTED === true) return true;
    if (typeof localStorage !== "undefined" && (localStorage.getItem("180hzdownloader_installed") === "true" || localStorage.getItem("turbodownloader_installed") === "true")) return true;
    return false;
  }

  const BACKEND_API_BASE = (function() {
    if (typeof window !== "undefined" && window.FLOWDOWN_API_URL) return window.FLOWDOWN_API_URL;
    if (typeof location !== "undefined" && (location.port === "4000" || location.hostname === "127.0.0.1" || location.hostname === "localhost")) return `${location.origin}/api/v1`;
    return "http://127.0.0.1:4000/api/v1";
  })();

  function updateDesktopUiBadges() {
    if (isDesktopEnvironment()) {
      const navAppBtn = $("#navDownloadAppBtn");
      if (navAppBtn) {
        navAppBtn.textContent = "⚡ Engine Active";
        navAppBtn.removeAttribute("download");
        navAppBtn.href = "#console";
        navAppBtn.title = "180Hz Downloader Desktop Engine is Connected & Ready";
        navAppBtn.style.background = "rgba(216,255,62,0.15)";
        navAppBtn.style.color = "var(--lime)";
        navAppBtn.style.border = "1px solid var(--lime)";
      }
      const heroAppBtn = $("#heroDownloadAppBtn");
      if (heroAppBtn) {
        heroAppBtn.innerHTML = "⚡ 180HZ ENGINE ACTIVE";
        heroAppBtn.removeAttribute("download");
        heroAppBtn.href = "#console";
      }
    }
  }

  // Proactively check if Desktop Engine is running locally on port 4000
  (async function checkDesktopEngine() {
    updateDesktopUiBadges();
    try {
      const ctrl = new AbortController();
      const t = setTimeout(() => ctrl.abort(), 1200);
      const res = await fetch("http://127.0.0.1:4000/api/v1/health", { mode: "cors", signal: ctrl.signal });
      clearTimeout(t);
      if (res && res.ok) {
        window.DESKTOP_CONNECTED = true;
        if (typeof localStorage !== "undefined") localStorage.setItem("180hzdownloader_installed", "true");
        updateDesktopUiBadges();
      }
    } catch {}
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
      { formatId: "best", quality: "Best Available", resolution: "Auto Max (4K/8K)", container: "mp4", fps: 60, vcodec: "H.264 / AV1", filesizeFormatted: "Source" },
      { formatId: "2160p", quality: "4K UHD", resolution: "2160p / 4K", container: "mp4", fps: 60, vcodec: "AV1 / VP9", filesizeFormatted: "High Bitrate" },
      { formatId: "2160p-webm", quality: "4K UHD", resolution: "2160p / 4K", container: "webm", fps: 60, vcodec: "VP9", filesizeFormatted: "High Bitrate" },
      { formatId: "1440p", quality: "2K QHD", resolution: "1440p", container: "mp4", fps: 60, vcodec: "VP9 / AVC", filesizeFormatted: "Standard" },
      { formatId: "1440p-webm", quality: "2K QHD", resolution: "1440p", container: "webm", fps: 60, vcodec: "VP9", filesizeFormatted: "Standard" },
      { formatId: "1080p", quality: "Full HD", resolution: "1080p", container: "mp4", fps: 60, vcodec: "H.264", filesizeFormatted: "1080p" },
      { formatId: "1080p-webm", quality: "Full HD", resolution: "1080p", container: "webm", fps: 60, vcodec: "VP9", filesizeFormatted: "1080p" },
      { formatId: "720p", quality: "HD", resolution: "720p", container: "mp4", fps: 30, vcodec: "H.264", filesizeFormatted: "720p" },
      { formatId: "720p-webm", quality: "HD", resolution: "720p", container: "webm", fps: 30, vcodec: "VP9", filesizeFormatted: "720p" },
      { formatId: "480p", quality: "Standard", resolution: "480p", container: "mp4", fps: 30, vcodec: "H.264", filesizeFormatted: "480p" },
      { formatId: "360p", quality: "Low", resolution: "360p", container: "mp4", fps: 30, vcodec: "H.264", filesizeFormatted: "360p" }
    ];

    const audioFormats = [
      { formatId: "mp3-320k", quality: "Lossless MP3", label: "320 kbps (Lossless MP3)", container: "mp3", bitrateKbps: 320, codec: "MP3", filesizeFormatted: "320k Lossless", audioOnly: true },
      { formatId: "mp3-192k", quality: "High Quality MP3", label: "192 kbps (High Quality MP3)", container: "mp3", bitrateKbps: 192, codec: "MP3", filesizeFormatted: "192k High", audioOnly: true },
      { formatId: "mp3-128k", quality: "Standard MP3", label: "128 kbps (Standard MP3)", container: "mp3", bitrateKbps: 128, codec: "MP3", filesizeFormatted: "128k Standard", audioOnly: true },
      { formatId: "mp3-64k", quality: "Compact MP3", label: "64 kbps (Voice / Compact MP3)", container: "mp3", bitrateKbps: 64, codec: "MP3", filesizeFormatted: "64k Compact", audioOnly: true },
      { formatId: "128k", quality: "Standard Quality", label: "Standard / 128 kbps", container: "m4a", bitrateKbps: 128, codec: "AAC", filesizeFormatted: "128k", audioOnly: true },
      { formatId: "opus", quality: "Opus Audio", label: "Opus / 160 kbps", container: "webm", bitrateKbps: 160, codec: "Opus", filesizeFormatted: "160k", audioOnly: true }
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
    const timer = setTimeout(() => ctrl.abort(), 25000);
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
      let videoFormats = Array.isArray(d.videoFormats) ? d.videoFormats : [];
      let audioFormats = Array.isArray(d.audioFormats) ? d.audioFormats : [];

      if (videoFormats.length === 0) {
        const defaults = getUnlockedDefaultFormats(provider);
        videoFormats = defaults.videoFormats;
      }

      const hasMp3 = audioFormats.some(f => (f.container || "").toLowerCase() === "mp3" || (f.formatId || "").startsWith("mp3"));
      if (!hasMp3) {
        const durSec = d.durationSeconds || 0;
        const mp3Defaults = [
          { formatId: "mp3-320k", quality: "Lossless MP3", label: "320 kbps (Lossless MP3)", container: "mp3", bitrateKbps: 320, codec: "MP3", filesizeFormatted: durSec > 0 ? humanBytes(durSec * (320 * 1024 / 8)) : "320k Lossless", audioOnly: true },
          { formatId: "mp3-192k", quality: "High Quality MP3", label: "192 kbps (High Quality MP3)", container: "mp3", bitrateKbps: 192, codec: "MP3", filesizeFormatted: durSec > 0 ? humanBytes(durSec * (192 * 1024 / 8)) : "192k High", audioOnly: true },
          { formatId: "mp3-128k", quality: "Standard MP3", label: "128 kbps (Standard MP3)", container: "mp3", bitrateKbps: 128, codec: "MP3", filesizeFormatted: durSec > 0 ? humanBytes(durSec * (128 * 1024 / 8)) : "128k Standard", audioOnly: true },
          { formatId: "mp3-64k", quality: "Compact MP3", label: "64 kbps (Voice / Compact MP3)", container: "mp3", bitrateKbps: 64, codec: "MP3", filesizeFormatted: durSec > 0 ? humanBytes(durSec * (64 * 1024 / 8)) : "64k Compact", audioOnly: true }
        ];
        audioFormats = [...mp3Defaults, ...audioFormats];
      }

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
    } else if (url.includes("instagram.com") || url.includes("facebook.com") || url.includes("fb.watch") || url.includes("tiktok.com")) {
      const match = /(?:reel|p|tv|shorts|watch\?v=|\/video\/)([A-Za-z0-9_\-]+)/.exec(url);
      if (match && match[1] && (title.startsWith("Universal") || title.startsWith("Social"))) {
        title = `${provider || "Media"} (${match[1]})`;
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
      isLocalBackend: isDesktopEnvironment(),
      note: "Universal stream unlocked — select format to download bit-exact media.",
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
              
              <div id="ytVideoSection">
                <div class="yt-sec-title">${IC.film}VIDEO (MP4 / WEBM)</div>
                <div id="ytVideoOptionsList"></div>
              </div>

              <div id="ytMp3Section">
                <div class="yt-sec-title" style="color:var(--lime);display:flex;align-items:center;justify-content:space-between;margin-top:16px;">
                  <span style="display:inline-flex;align-items:center;gap:8px;">
                    <svg viewBox="0 0 24 24" class="ico" fill="none" stroke="currentColor" stroke-width="2" style="width:14px;height:14px;"><path d="M9 18V5l12-2v13"/><circle cx="6" cy="18" r="3"/><circle cx="18" cy="16" r="3"/></svg>
                    MP3 AUDIO (HIGH BITRATE / CONVERT)
                  </span>
                  <span class="yt-opt-pill best" style="font-size:9px;">ALL PLATFORMS</span>
                </div>
                <div id="ytMp3OptionsList"></div>
              </div>

              <div id="ytOtherAudioSection" class="hidden">
                <div class="yt-sec-title" style="color:var(--dim);margin-top:14px;">${IC.audio}ORIGINAL AUDIO STREAMS (M4A / WEBM)</div>
                <div id="ytOtherAudioOptionsList"></div>
              </div>
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
    const videoSection = $("#ytVideoSection");
    const videoList = $("#ytVideoOptionsList");
    const mp3Section = $("#ytMp3Section");
    const mp3List = $("#ytMp3OptionsList");
    const otherAudioSection = $("#ytOtherAudioSection");
    const otherAudioList = $("#ytOtherAudioOptionsList");
    const errContainer = $("#ytErrorContainer");

    let isDownloading = false;
    let pollTimer = null;
    let currentJobId = null;

    const videoFormats = Array.isArray(r.videoFormats) ? r.videoFormats : [];
    let audioFormats = Array.isArray(r.audioFormats) ? [...r.audioFormats] : [];

    const hasMp3InList = audioFormats.some(f => (f.container || "").toLowerCase() === "mp3" || (f.formatId || "").startsWith("mp3"));
    if (!hasMp3InList) {
      const durSec = r.durationSeconds || 0;
      const mp3Defaults = [
        { formatId: "mp3-320k", label: "320 kbps (Lossless MP3)", container: "mp3", bitrateKbps: 320, codec: "MP3", filesizeFormatted: durSec > 0 ? humanBytes(durSec * (320 * 1024 / 8)) : "320k Lossless", audioOnly: true },
        { formatId: "mp3-192k", label: "192 kbps (High Quality MP3)", container: "mp3", bitrateKbps: 192, codec: "MP3", filesizeFormatted: durSec > 0 ? humanBytes(durSec * (192 * 1024 / 8)) : "192k High", audioOnly: true },
        { formatId: "mp3-128k", label: "128 kbps (Standard MP3)", container: "mp3", bitrateKbps: 128, codec: "MP3", filesizeFormatted: durSec > 0 ? humanBytes(durSec * (128 * 1024 / 8)) : "128k Standard", audioOnly: true },
        { formatId: "mp3-64k", label: "64 kbps (Voice / Compact MP3)", container: "mp3", bitrateKbps: 64, codec: "MP3", filesizeFormatted: durSec > 0 ? humanBytes(durSec * (64 * 1024 / 8)) : "64k Compact", audioOnly: true }
      ];
      audioFormats = [...mp3Defaults, ...audioFormats];
    }

    const mp3Formats = audioFormats.filter(f => (f.container || "").toLowerCase() === "mp3" || (f.formatId || "").startsWith("mp3"));
    const otherAudioFormats = audioFormats.filter(f => (f.container || "").toLowerCase() !== "mp3" && !(f.formatId || "").startsWith("mp3"));

    const containers = Array.from(new Set([
      ...videoFormats.map((f) => (f.container || "").toLowerCase()),
      "mp3",
      ...otherAudioFormats.map((f) => (f.container || "").toLowerCase())
    ])).filter(Boolean);

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
    } else if (mp3Formats.length > 0) {
      selectedFormat = {
        ...mp3Formats[0],
        audioOnly: true,
        isBest: true,
        label: `320 kbps MP3`
      };
    }
    updateButtonLabel();

    function renderOptions() {
      // 1. Video Section
      const shouldShowVideo = activeFilter === "ALL" || activeFilter === "MP4" || activeFilter === "WEBM";
      if (!shouldShowVideo) {
        if (videoSection) videoSection.classList.add("hidden");
      } else {
        if (videoSection) videoSection.classList.remove("hidden");
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
                <span class="yt-opt-action">${isBestSelected ? `${IC.zap} Ready` : `${IC.dl} Select`}</span>
              </div>
            </div>
          `;

          filteredVideo.forEach((fmt, idx) => {
            if (fmt.formatId === "best") return;
            const isSelected = selectedFormat && !selectedFormat.isBest && !selectedFormat.audioOnly && selectedFormat.formatId === fmt.formatId;
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
                  <span class="yt-opt-action">${isSelected ? `${IC.zap} Ready` : `${IC.dl} Select`}</span>
                </div>
              </div>
            `;
          });
        } else {
          videoHtml = `<div style="font-family:var(--font-m);font-size:11px;color:var(--dim);padding:8px 4px;">No video streams matching filter.</div>`;
        }
        videoList.innerHTML = videoHtml;
      }

      // 2. Dedicated MP3 Section
      const shouldShowMp3 = activeFilter === "ALL" || activeFilter === "MP3";
      if (!shouldShowMp3 || mp3Formats.length === 0) {
        if (mp3Section) mp3Section.classList.add("hidden");
      } else {
        if (mp3Section) mp3Section.classList.remove("hidden");
        let mp3Html = "";
        mp3Formats.forEach((fmt, idx) => {
          const isSelected = selectedFormat && selectedFormat.audioOnly && (selectedFormat.container || "").toLowerCase() === "mp3" &&
            (selectedFormat.formatId === fmt.formatId || (selectedFormat.isBest && idx === 0));
          const size = fmt.filesizeFormatted || (fmt.estimatedSizeBytes ? humanBytes(fmt.estimatedSizeBytes) : null);
          const badge = fmt.bitrateKbps >= 320 ? "STUDIO / LOSSLESS" : (fmt.bitrateKbps >= 192 ? "HIGH QUALITY" : (fmt.bitrateKbps >= 128 ? "STANDARD" : "COMPACT"));

          mp3Html += `
            <div class="yt-opt-item ${isSelected ? "selected" : ""}" role="option" tabindex="0"
                 data-opt-type="mp3" data-idx="${idx}" aria-selected="${isSelected ? "true" : "false"}">
              <div class="yt-opt-main">
                <span class="yt-opt-label">${escapeHtml(fmt.label || `${fmt.bitrateKbps} kbps MP3`)}</span>
                <span class="yt-opt-pill best">MP3</span>
                <span class="yt-opt-pill ${fmt.bitrateKbps >= 320 ? "best" : ""}">${fmt.bitrateKbps ? `${fmt.bitrateKbps} KBPS` : "320 KBPS"}</span>
                <span class="yt-opt-pill">${badge}</span>
              </div>
              <div class="yt-opt-meta">
                ${size ? `<span class="yt-opt-size">${escapeHtml(size)}</span>` : ""}
                <span class="yt-opt-action">${isSelected ? `${IC.zap} Ready` : `${IC.dl} Select`}</span>
              </div>
            </div>
          `;
        });
        mp3List.innerHTML = mp3Html;
      }

      // 3. Other Audio Streams (M4A / Opus / WebM)
      const shouldShowOtherAudio = activeFilter === "ALL" || activeFilter === "M4A" || activeFilter === "WEBM" || activeFilter === "OPUS";
      const filteredOther = activeFilter === "ALL"
        ? otherAudioFormats
        : otherAudioFormats.filter((f) => (f.container || "").toUpperCase() === activeFilter);

      if (!shouldShowOtherAudio || filteredOther.length === 0) {
        if (otherAudioSection) otherAudioSection.classList.add("hidden");
      } else {
        if (otherAudioSection) otherAudioSection.classList.remove("hidden");
        let otherHtml = "";
        filteredOther.forEach((fmt, idx) => {
          const isSelected = selectedFormat && selectedFormat.audioOnly && (selectedFormat.container || "").toLowerCase() !== "mp3" && selectedFormat.formatId === fmt.formatId;
          const size = fmt.filesizeFormatted || (fmt.estimatedSizeBytes ? humanBytes(fmt.estimatedSizeBytes) : null);
          const label = fmt.label || fmt.formatId;

          otherHtml += `
            <div class="yt-opt-item ${isSelected ? "selected" : ""}" role="option" tabindex="0"
                 data-opt-type="other-audio" data-idx="${idx}" aria-selected="${isSelected ? "true" : "false"}">
              <div class="yt-opt-main">
                <span class="yt-opt-label">${escapeHtml(label)}</span>
                ${fmt.bitrateKbps ? `<span class="yt-opt-pill">${fmt.bitrateKbps} kbps</span>` : ""}
                ${fmt.codec ? `<span class="yt-opt-pill">${escapeHtml(cleanCodec(fmt.codec))}</span>` : ""}
                ${fmt.container ? `<span class="yt-opt-pill">${escapeHtml(fmt.container.toUpperCase())}</span>` : ""}
              </div>
              <div class="yt-opt-meta">
                ${size ? `<span class="yt-opt-size">${escapeHtml(size)}</span>` : ""}
                <span class="yt-opt-action">${isSelected ? `${IC.zap} Ready` : `${IC.dl} Select`}</span>
              </div>
            </div>
          `;
        });
        otherAudioList.innerHTML = otherHtml;
      }

      // Handle selection click & keyboard
      $$(".yt-opt-item", panel).forEach((el) => {
        const handlePick = (e) => {
          const type = el.dataset.optType;
          let wasAlreadySelected = false;

          if (type === "video-best") {
            const filteredVideo = activeFilter === "ALL"
              ? videoFormats
              : videoFormats.filter((f) => (f.container || "").toUpperCase() === activeFilter);
            const best = filteredVideo[0];
            wasAlreadySelected = selectedFormat && selectedFormat.isBest && !selectedFormat.audioOnly;
            selectedFormat = { ...best, isBest: true, audioOnly: false, label: `Best (${getCanonicalResolution(best)})` };
          } else if (type === "video") {
            const filteredVideo = activeFilter === "ALL"
              ? videoFormats
              : videoFormats.filter((f) => (f.container || "").toUpperCase() === activeFilter);
            const fmt = filteredVideo[Number(el.dataset.idx)];
            wasAlreadySelected = selectedFormat && !selectedFormat.isBest && !selectedFormat.audioOnly && selectedFormat.formatId === fmt.formatId;
            selectedFormat = { ...fmt, isBest: false, audioOnly: false, label: getCanonicalResolution(fmt) };
          } else if (type === "mp3") {
            const fmt = mp3Formats[Number(el.dataset.idx)];
            wasAlreadySelected = selectedFormat && selectedFormat.audioOnly && (selectedFormat.container || "").toLowerCase() === "mp3" && selectedFormat.formatId === fmt.formatId;
            selectedFormat = { ...fmt, audioOnly: true, isBest: Number(el.dataset.idx) === 0, container: "mp3", label: fmt.label || `${fmt.bitrateKbps} kbps MP3` };
          } else if (type === "other-audio") {
            const filteredOther = activeFilter === "ALL"
              ? otherAudioFormats
              : otherAudioFormats.filter((f) => (f.container || "").toUpperCase() === activeFilter);
            const fmt = filteredOther[Number(el.dataset.idx)];
            wasAlreadySelected = selectedFormat && selectedFormat.audioOnly && selectedFormat.formatId === fmt.formatId;
            selectedFormat = { ...fmt, audioOnly: true, isBest: false, label: fmt.label || fmt.formatId };
          }

          updateButtonLabel();
          renderOptions();

          const isActionClick = e && e.target && (e.target.closest(".yt-opt-action") || e.target.classList.contains("yt-opt-action"));
          if (wasAlreadySelected || isActionClick) {
            if (r.isLocalBackend || isDesktopEnvironment()) {
              executeDownload(selectedFormat);
            } else {
              handleLaunchOrInstallDesktop(r.url);
            }
          }
        };

        el.addEventListener("click", handlePick);
        el.addEventListener("keydown", (e) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            handlePick(e);
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

    function updateButtonLabel() {
      const selName = selectedFormat ? selectedFormat.label : "Video";
      const cont = selectedFormat && selectedFormat.container ? ` [${selectedFormat.container.toUpperCase()}]` : "";
      text.textContent = `⚡ Start Download — ${selName}${cont}`;
    }

    function togglePanel(open) {
      const willOpen = typeof open === "boolean" ? open : panel.classList.contains("hidden");
      panel.classList.toggle("hidden", !willOpen);
      btn.setAttribute("aria-expanded", String(willOpen));
      updateButtonLabel();

      if (willOpen) {
        renderOptions();
        const firstOpt = $(".yt-opt-item", panel);
        if (firstOpt) setTimeout(() => firstOpt.focus(), 50);
      }
    }

    const downloadedCache = {};

    function showCompletedUi(completedJob, fmt) {
      isDownloading = false;
      btn.disabled = false;
      cancel.classList.add("hidden");
      fill.style.width = "100%";
      text.textContent = "✓ Saved — Click for Options";
      msg.className = "msg ok";
      const sizeLabel = completedJob.fileSizeBytes ? humanBytes(completedJob.fileSizeBytes) : (completedJob.downloadedBytes ? humanBytes(completedJob.downloadedBytes) : "");
      const sizeText = sizeLabel ? ` (${sizeLabel})` : "";
      const currentFmt = fmt || selectedFormat;
      const isAudio = Boolean(currentFmt && (currentFmt.audioOnly || (currentFmt.container || "").toLowerCase() === "mp3" || (currentFmt.formatId || "").startsWith("mp3")));
      const openLabel = isAudio ? "🎵 Open Audio" : "🎬 Open Video";
      const cacheKey = `${r.url}_${(currentFmt && currentFmt.formatId) || 'best'}_${(currentFmt && currentFmt.container) || 'mp4'}_${Boolean(currentFmt && currentFmt.audioOnly)}`;
      const mediaFileName = `${completedJob.title || r.title || (isAudio ? "audio" : "video")}.${completedJob.requestedFormat || (isAudio ? "mp3" : "mp4")}`;

      msg.innerHTML = `
        <div class="post-dl-card">
          <div class="post-dl-header">
            <span class="post-dl-check">✓</span>
            <span class="post-dl-title" title="${escapeHtml(completedJob.title || r.title || (isAudio ? 'Audio' : 'Video'))}">
              Saved: <strong>${escapeHtml(completedJob.title || r.title || (isAudio ? "Audio" : "Video"))}</strong>
            </span>
            ${sizeText ? `<span class="post-dl-size">${sizeText}</span>` : ""}
          </div>
          <div class="post-dl-actions">
            <button type="button" class="post-dl-btn post-dl-btn-primary" id="openVideoFileBtn" title="${openLabel}">
              ${openLabel}
            </button>
            <button type="button" class="post-dl-btn post-dl-btn-view" id="openLocalFolderBtn" title="Highlight and view file in folder">
              📂 View File
            </button>
            <button type="button" class="post-dl-btn post-dl-btn-save" id="saveAsBtn" title="Save file to custom folder">
              💾 Save As
            </button>
            <button type="button" class="post-dl-btn post-dl-btn-redownload" id="reDownloadBtn" title="Force fresh re-download">
              🔄 Re-download
            </button>
          </div>
        </div>
      `;

      let fileUrl = completedJob.downloadUrl || `${BACKEND_API_BASE}/downloads/${completedJob.id}/file`;
      if (!fileUrl.startsWith("http")) {
        const apiOrigin = new URL(BACKEND_API_BASE).origin;
        fileUrl = `${apiOrigin}${fileUrl.startsWith('/') ? '' : '/'}${fileUrl}`;
      }

      setTimeout(() => {
        const ovb = document.getElementById("openVideoFileBtn");
        if (ovb) {
          ovb.addEventListener("click", async (e) => {
            e.preventDefault();
            if (r.isLocalBackend || isDesktopEnvironment()) {
              try {
                const res = await fetch(`${BACKEND_API_BASE}/downloads/${completedJob.id}/open?target=file`, { method: "POST" });
                if (!res.ok) throw new Error("Local open failed");
              } catch (err) {
                window.open(fileUrl, "_blank");
              }
            } else {
              window.open(fileUrl, "_blank");
            }
          });
        }

        const ofb = document.getElementById("openLocalFolderBtn");
        if (ofb) {
          ofb.addEventListener("click", async (e) => {
            e.preventDefault();
            if (r.isLocalBackend || isDesktopEnvironment()) {
              try {
                const res = await fetch(`${BACKEND_API_BASE}/downloads/${completedJob.id}/open`, { method: "POST" });
                if (!res.ok) throw new Error("Local folder open failed");
              } catch (err) {
                window.open(fileUrl, "_blank");
              }
            } else {
              window.open(fileUrl, "_blank");
            }
          });
        }

        const sab = document.getElementById("saveAsBtn");
        if (sab) {
          sab.addEventListener("click", async (e) => {
            e.preventDefault();
            await triggerSaveAs(fileUrl, mediaFileName);
          });
        }

        const rdb = document.getElementById("reDownloadBtn");
        if (rdb) {
          rdb.addEventListener("click", (e) => {
            e.preventDefault();
            delete downloadedCache[cacheKey];
            delete downloadedCache[r.url];
            msg.className = "msg";
            msg.innerHTML = "";
            executeDownload(currentFmt || selectedFormat, true);
          });
        }
      }, 50);

      // ONLY trigger browser nativeHandoff if purely on remote web (no local desktop engine saving to disk)
      if (!isDesktopEnvironment() && !r.isLocalBackend) {
        nativeHandoff(fileUrl, mediaFileName);
      }

      Ledger.bump(r.url);
    }

    btn.addEventListener("click", () => {
      if (isDownloading) return;
      if (r.isLocalBackend || isDesktopEnvironment()) {
        executeDownload(selectedFormat);
      } else {
        handleLaunchOrInstallDesktop(r.url);
      }
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

      const cacheKey = `${r.url}_${(fmt && fmt.formatId) || 'best'}_${(fmt && fmt.container) || 'mp4'}_${Boolean(fmt && fmt.audioOnly)}`;

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
        audioOnly: Boolean(fmt.audioOnly),
        force: true
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
              downloadedCache[cacheKey] = currentJob;
              downloadedCache[r.url] = currentJob;
              showCompletedUi(currentJob, fmt);

            } else if (currentJob.status === "FAILED" || currentJob.status === "CANCELLED") {
              clearInterval(pollTimer);
              pollTimer = null;
              isDownloading = false;
              btn.disabled = false;
              cancel.classList.add("hidden");
              fill.style.width = "0%";
              text.textContent = "Download Video ▼";

              const errMsg = currentJob.errorMessage || "Processing failed during download.";
              const isPlatformProtected = errMsg.includes("empty media response") || errMsg.includes("cookies") || errMsg.includes("login") || errMsg.includes("bot");
              const tunnelUrl = getWebTunnelUrl(r.url, r.provider);

              errContainer.innerHTML = buildErrorCardHtml({
                title: isPlatformProtected ? "Platform Access Restricted" : "Download Failed",
                cause: errMsg,
                code: currentJob.errorCode || "DL_FAILED",
                retryId: "errRetryBtn",
                retryText: "🔄 Retry Download",
                tunnelUrl: tunnelUrl,
                url: r.url,
                showAppDownload: true
              });

              setTimeout(() => {
                const retryBtn = document.getElementById("errRetryBtn");
                if (retryBtn) {
                  retryBtn.addEventListener("click", () => {
                    executeDownload(selectedFormat, true);
                  });
                }
              }, 50);
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
        fill.style.width = "0%";
        updateButtonLabel();

        const errMsg = (err && err.message) ? err.message : "Failed to connect to local engine.";
        const tunnelUrl = getWebTunnelUrl(r.url, r.provider);

        errContainer.innerHTML = buildErrorCardHtml({
          title: "Download Initiation Failed",
          cause: errMsg,
          code: err.code || "INIT_FAILED",
          retryId: "retryDownloadBtn",
          retryText: "🔄 Retry Download",
          tunnelUrl: tunnelUrl,
          url: r.url,
          showAppDownload: true
        });

        setTimeout(() => {
          const rb = document.getElementById("retryDownloadBtn");
          if (rb) rb.addEventListener("click", () => executeDownload(selectedFormat));
        }, 50);

        if (!isDesktopEnvironment() && !r.isLocalBackend) {
          renderDesktopInstallerCard(fmt, true);
        }
        Ledger.bump(r.url);
      }
    }

    async function handleLaunchOrInstallDesktop(mediaUrl) {
      const isLocal = window.location.port === "4000" || window.location.hostname === "127.0.0.1" || window.location.hostname === "localhost";
      if (isLocal) {
        window.location.href = `http://127.0.0.1:4000/?url=${encodeURIComponent(mediaUrl)}`;
        return;
      }

      let isRunning = false;
      try {
        const ctrl = new AbortController();
        const t = setTimeout(() => ctrl.abort(), 1200);
        const res = await fetch("http://127.0.0.1:4000/api/v1/health", { mode: "cors", signal: ctrl.signal });
        clearTimeout(t);
        if (res && res.ok) isRunning = true;
      } catch {}

      if (isRunning) {
        window.open(`http://127.0.0.1:4000/?url=${encodeURIComponent(mediaUrl)}`, "_blank");
      } else {
        if (typeof localStorage !== "undefined") {
          localStorage.setItem("180hzdownloader_installed", "true");
        }
        // Automatically trigger 1-click download of 180Hz Downloader Setup.exe
        const a = document.createElement("a");
        a.href = "180Hz Downloader Setup.exe";
        a.download = "180Hz Downloader Setup.exe";
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);

        renderDesktopInstallerCard(selectedFormat);
      }
    }

    function renderDesktopInstallerCard(fmt, appendMode = false) {
      if (isDesktopEnvironment() || r.isLocalBackend) return;

      const formatLabel = fmt ? (fmt.label || getCanonicalResolution(fmt) || "Lossless 4K/8K") : "Lossless 4K/8K";

      const cardHtml = `
        <div class="desktop-installer-card" style="margin-top:16px;background:rgba(124,92,255,0.08);border:1px solid rgba(124,92,255,0.45);border-radius:12px;padding:20px;animation:card-in .4s ease;">
          <div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:12px;flex-wrap:wrap;gap:8px;">
            <div style="display:flex;align-items:center;gap:8px;">
              ${IC.zap}
              <strong style="color:var(--lime);font-size:12px;letter-spacing:.12em;text-transform:uppercase;">180HZ DOWNLOADER 8K DESKTOP ENGINE</strong>
            </div>
            <span class="tag done" style="font-size:10px;background:rgba(216,255,62,0.15);color:var(--lime);border:1px solid rgba(216,255,62,0.3);">${escapeHtml(formatLabel)}</span>
          </div>
          <p style="font-size:13px;color:#edece8;margin-bottom:14px;line-height:1.6;">
            To download <strong>${escapeHtml(r.title || "this media")}</strong> in full bit-exact 4K/8K quality with untouched lossless audio, use the 1-click Windows desktop app:
          </p>
          <div style="display:flex;align-items:center;gap:12px;flex-wrap:wrap;margin-bottom:14px;">
            <a href="180Hz Downloader Setup.exe" download="180Hz Downloader Setup.exe" class="chip" style="background:var(--lime);color:#000;font-weight:700;font-size:12px;padding:10px 22px;text-decoration:none;border-radius:8px;display:inline-flex;align-items:center;gap:8px;box-shadow:0 0 20px rgba(216,255,62,0.25);">
              ${IC.dl} Download Windows App (.exe)
            </a>
            <button type="button" class="chip openTurboDesktopBtn" style="background:rgba(124,92,255,0.25);color:#c4b5fd;border:1px solid rgba(124,92,255,0.6);font-size:11px;padding:10px 18px;cursor:pointer;display:inline-flex;align-items:center;gap:6px;border-radius:8px;">
              ${IC.zap} Open in 180Hz Downloader Desktop (Lossless 4K/8K)
            </button>
          </div>
          <div style="padding-top:12px;border-top:1px solid rgba(255,255,255,0.06);font-size:11px;color:rgba(237,237,232,0.7);display:flex;gap:16px;flex-wrap:wrap;">
            <span>✓ Auto-installs yt-dlp & FFmpeg</span>
            <span>✓ 100% Lossless 4K & 8K Video</span>
            <span>✓ Untouched Hi-Fi Audio</span>
          </div>
        </div>
      `;

      if (appendMode) {
        const wrap = document.createElement("div");
        wrap.innerHTML = cardHtml;
        errContainer.appendChild(wrap);
      } else {
        errContainer.innerHTML = cardHtml;
      }

      setTimeout(() => {
        const openDesktopBtns = errContainer.querySelectorAll(".openTurboDesktopBtn, #openTurboDesktopBtn");
        openDesktopBtns.forEach(btnEl => {
          btnEl.addEventListener("click", () => {
            handleLaunchOrInstallDesktop(r.url);
          });
        });
      }, 0);

      fill.style.width = "100%";
      text.textContent = "Download via App (.exe) ⚡";
      isDownloading = false;
      btn.disabled = false;
      cancel.classList.add("hidden");
    }

    renderOptions();
    panel.classList.remove("hidden");
    btn.setAttribute("aria-expanded", "true");
    updateButtonLabel();

    if (!r.isLocalBackend && !isDesktopEnvironment()) {
      renderDesktopInstallerCard(selectedFormat);
    }
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
    if (document.body.classList.contains("version-locked")) {
      const gate = document.getElementById("updateGateCard");
      if (gate) gate.scrollIntoView({ behavior: "smooth", block: "center" });
      showError("This version has been deprecated. Please click UPDATE NOW to continue.", {
        title: "Update Required",
        code: "VERSION_OUTDATED"
      });
      return;
    }
    let clean = String(raw || (input ? input.value : "") || "").trim();
    if (!clean) {
      if (input) {
        input.focus();
        input.classList.add("input-shake");
        setTimeout(() => input.classList.remove("input-shake"), 500);
      }
      showError("Please paste a YouTube, Facebook, or Instagram video link first.", {
        title: "No Link Provided",
        code: "EMPTY_INPUT"
      });
      return;
    }

    if (probing) return;

    // Smart URL extraction & protocol normalization
    const extracted = clean.match(/https?:\/\/[^\s"']+/i);
    if (extracted) {
      clean = extracted[0];
    } else if (!/^https?:\/\//i.test(clean)) {
      if (/^(www\.|youtube\.com|youtu\.be|instagram\.com|facebook\.com|fb\.watch)/i.test(clean)) {
        clean = "https://" + clean;
      }
    }

    const u = parseUrl(clean);
    zone.innerHTML = "";
    if (!u) {
      if (input) {
        input.focus();
        input.classList.add("input-shake");
        setTimeout(() => input.classList.remove("input-shake"), 500);
      }
      showError("Please enter a valid link from YouTube, Facebook, or Instagram.", {
        title: "Invalid Link Format",
        code: "INVALID_URL",
        url: clean
      });
      return;
    }

    if (input) input.value = clean;
    updateInputUi();

    probing = true;
    btn.disabled = true;
    btn.classList.add("busy");
    btn.classList.remove("ready");
    label.textContent = "Analyzing…";
    startSteps();
    const t0 = Date.now();
    try {
      let result;
      const kind = classify(u);
      const prov = kind.charAt(0).toUpperCase() + kind.slice(1);
      result = await analyzeViaBackend(clean, prov);

      await new Promise((r) => setTimeout(r, Math.max(0, 150 - (Date.now() - t0))));
      Ledger.add({
        url: clean, kind: result.kind, provider: result.provider,
        title: result.title || result.filename || null,
        ct: result.contentType || null, size: result.sizeBytes ?? null,
        status: "analyzed", t: Date.now(),
      });
      if (result.type === "youtube-download" || result.type === "embed") renderYouTubeCard(result);
      else renderDirect(result);
    } catch (e) {
      showError(e instanceof Error ? e.message : "Could not analyze that link.", {
        title: "Link Analysis Failed",
        code: "ANALYZE_ERROR",
        url: clean
      });
    } finally {
      stopSteps();
      probing = false;
      btn.disabled = false;
      btn.classList.remove("busy");
      label.textContent = "Analyze";
      updateInputUi();
    }
  }

  form.addEventListener("submit", (e) => {
    e.preventDefault();
    analyze(input ? input.value : "");
  });

  if (btn) {
    btn.addEventListener("click", (e) => {
      e.preventDefault();
      analyze(input ? input.value : "");
    });
  }

  /* paste anywhere → capture to input box ONLY (manual analyze) */
  addEventListener("paste", (e) => {
    if (document.activeElement === input) return;
    const text = (e.clipboardData || window.clipboardData).getData("text").trim();
    if (/^https?:\/\/\S+$/i.test(text) || /(?:youtube\.com|youtu\.be|instagram\.com|facebook\.com|fb\.watch)/i.test(text)) {
      input.value = text;
      updateInputUi();
      input.focus();
      highlightAnalyzeReady();
      // Manual only: user must click Analyze
    }
  });

  /* ledger re-download -> populate URL input & analyze directly to download options */
  document.addEventListener("click", (e) => {
    const b = e.target.closest("[data-re-dl]");
    if (!b) return;
    const targetUrl = decodeURIComponent(b.dataset.reDl || "");
    if (!targetUrl) return;

    // Ensure lamp is turned on if still off
    if (document.body.classList.contains("lamp-off")) {
      document.documentElement.classList.remove("lamp-off");
      document.body.classList.remove("lamp-off");
      document.body.classList.add("lamp-on");
      window.updateBeamGeometry?.();
    }

    // Populate URL input
    if (input) {
      input.value = targetUrl;
      updateInputUi();
    }

    // Smooth scroll to console area so user sees the analysis & options
    const consoleSec = document.getElementById("console");
    if (consoleSec) {
      consoleSec.scrollIntoView({ behavior: "smooth", block: "start" });
    }

    // Trigger analysis immediately to reveal all download options
    analyze(targetUrl);
    Ledger.bump(targetUrl);
  });

  /* ledger open external video link */
  document.addEventListener("click", (e) => {
    const linkBtn = e.target.closest("[data-open-external]");
    if (!linkBtn) return;
    const url = decodeURIComponent(linkBtn.dataset.openExternal || linkBtn.href || "");
    if (!url) return;

    const isLocalApp = (typeof location !== "undefined" && (location.port === "4000" || location.hostname === "127.0.0.1" || location.hostname === "localhost"));
    if (isLocalApp) {
      e.preventDefault();
      fetch(`${BACKEND_API_BASE}/system/open-url`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ url })
      }).catch(() => {
        window.open(url, "_blank");
      });
    }
  });

  /* ledger clear history */
  const clearLedgerBtn = document.getElementById("clearLedgerBtn");
  if (clearLedgerBtn) {
    clearLedgerBtn.addEventListener("click", (e) => {
      e.preventDefault();
      Ledger.clear();
    });
  }

  Ledger.render();

  /* populate url param (e.g. ?url=...) without auto-analyzing */
  try {
    const params = new URLSearchParams(window.location.search);
    const initialUrl = params.get("url");
    if (initialUrl && initialUrl.trim()) {
      input.value = initialUrl.trim();
      updateInputUi();
      highlightAnalyzeReady();
      // Manual only: user must click Analyze
    }
  } catch {}

  /* open official platform links in default OS browser when in desktop mode */
  document.addEventListener("click", (e) => {
    const link = e.target.closest("a.keyword-chip.platform");
    if (link && link.href) {
      const isLocalApp = (typeof location !== "undefined" && (location.port === "4000" || location.hostname === "127.0.0.1" || location.hostname === "localhost"));
      if (isLocalApp) {
        e.preventDefault();
        fetch(`${BACKEND_API_BASE}/system/open-url`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ url: link.href })
        }).catch(() => {
          window.open(link.href, "_blank");
        });
      }
    }
  });

  /* ========================================================
     STUDIO LAMP THEME CONTROLLER
     Direct Lamp Interface on Startup -> Click Button to Open
     ======================================================== */
  (function initStudioLamp() {
    const lampStage = document.getElementById("lampStage");
    if (!lampStage) return;

    const pullChain = document.getElementById("lampPullChain");
    const lampHead = document.getElementById("lampClickableHead");
    const lampPowerBtn = document.getElementById("lampPowerBtn");
    const standbyHint = document.getElementById("lampStandbyHint");

    let isLampLit = false;

    function playLampSound(on) {
      try {
        const AudioCtx = window.AudioContext || window.webkitAudioContext;
        if (!AudioCtx) return;
        const ctx = new AudioCtx();
        if (ctx.state === "suspended") {
          ctx.resume().catch(() => {});
        }
        const now = ctx.currentTime;

        // 1. Tactile Impact Transient (Crisp filtered mechanical friction snap)
        const noiseDuration = 0.015;
        const sampleRate = ctx.sampleRate || 44100;
        const frameCount = Math.floor(sampleRate * noiseDuration);
        const noiseBuffer = ctx.createBuffer(1, frameCount, sampleRate);
        const channelData = noiseBuffer.getChannelData(0);
        for (let i = 0; i < frameCount; i++) {
          channelData[i] = (Math.random() * 2 - 1) * (1 - i / frameCount);
        }

        const noiseSource = ctx.createBufferSource();
        noiseSource.buffer = noiseBuffer;

        const noiseFilter = ctx.createBiquadFilter();
        noiseFilter.type = "bandpass";
        noiseFilter.frequency.setValueAtTime(on ? 2800 : 2200, now);
        noiseFilter.Q.setValueAtTime(2.0, now);

        const noiseGain = ctx.createGain();
        noiseGain.gain.setValueAtTime(0.38, now);
        noiseGain.gain.exponentialRampToValueAtTime(0.001, now + noiseDuration);

        noiseSource.connect(noiseFilter);
        noiseFilter.connect(noiseGain);
        noiseGain.connect(ctx.destination);
        noiseSource.start(now);

        // 2. Solid Body Thud (Realistic acoustic mechanical switch body)
        const bodyOsc = ctx.createOscillator();
        const bodyGain = ctx.createGain();
        bodyOsc.type = "sine";
        const startFreq = on ? 440 : 360;
        const endFreq = on ? 110 : 85;
        bodyOsc.frequency.setValueAtTime(startFreq, now);
        bodyOsc.frequency.exponentialRampToValueAtTime(endFreq, now + 0.035);

        bodyGain.gain.setValueAtTime(0.26, now);
        bodyGain.gain.exponentialRampToValueAtTime(0.001, now + 0.035);

        bodyOsc.connect(bodyGain);
        bodyGain.connect(ctx.destination);
        bodyOsc.start(now);
        bodyOsc.stop(now + 0.038);

        // 3. Tactile Mechanical Latch Rebound (Detent settlement ~22ms later)
        const reboundTime = now + 0.022;
        const reboundOsc = ctx.createOscillator();
        const reboundGain = ctx.createGain();
        reboundOsc.type = "sine";
        reboundOsc.frequency.setValueAtTime(on ? 680 : 540, reboundTime);
        reboundOsc.frequency.exponentialRampToValueAtTime(130, reboundTime + 0.018);

        reboundGain.gain.setValueAtTime(0.08, reboundTime);
        reboundGain.gain.exponentialRampToValueAtTime(0.001, reboundTime + 0.018);

        reboundOsc.connect(reboundGain);
        reboundGain.connect(ctx.destination);
        reboundOsc.start(reboundTime);
        reboundOsc.stop(reboundTime + 0.022);

        // Clean up AudioContext lifecycle to prevent audio device leaks
        setTimeout(() => {
          try {
            if (ctx.state !== "closed") {
              ctx.close().catch(() => {});
            }
          } catch (e) {}
        }, 150);
      } catch {}
    }

    function setLampState(on, animate = true) {
      isLampLit = on;

      if (animate) {
        if (pullChain) {
          pullChain.classList.remove("chain-pulled");
          void pullChain.offsetWidth; // trigger reflow
          pullChain.classList.add("chain-pulled");
          setTimeout(() => pullChain.classList.remove("chain-pulled"), 450);
        }
        playLampSound(on);
      }

      if (on) {
        document.documentElement.classList.remove("lamp-off");
        document.body.classList.remove("lamp-off");
        document.body.classList.add("lamp-on");
        updateBeamGeometry();
      } else {
        document.body.classList.remove("lamp-on");
        document.documentElement.classList.add("lamp-off");
        document.body.classList.add("lamp-off");
        updateBeamGeometry();
      }
    }

    function updateBeamGeometry() {
      const cone = document.getElementById("lampBeamCone");
      const pool = document.getElementById("lampAmbientPool");
      const workspace = document.getElementById("mainAppWorkspace");
      if (!cone || !workspace) return;
      if (!document.body.classList.contains("lamp-on")) {
        cone.style.height = "";
        if (pool) pool.style.height = "";
        return;
      }
      const targetH = Math.max(window.innerHeight, workspace.offsetHeight + 180);
      cone.style.height = `${targetH}px`;
      if (pool) pool.style.height = `${targetH}px`;
    }
    window.updateBeamGeometry = updateBeamGeometry;

    if (window.ResizeObserver) {
      const ro = new ResizeObserver(() => {
        if (document.body.classList.contains("lamp-on")) {
          updateBeamGeometry();
        }
      });
      const ws = document.getElementById("mainAppWorkspace");
      if (ws) ro.observe(ws);
    }
    window.addEventListener("resize", updateBeamGeometry, { passive: true });

    // Default startup state: Direct Lamp Interface, Scroll Locked (Lamp OFF)
    document.documentElement.classList.add("lamp-off");
    document.body.classList.add("lamp-off");
    document.body.classList.remove("lamp-on");

    // Click handlers for Direct Open
    if (lampPowerBtn) {
      lampPowerBtn.addEventListener("click", (e) => {
        e.stopPropagation();
        setLampState(true, true);
      });
    }

    if (standbyHint) {
      standbyHint.addEventListener("click", (e) => {
        e.stopPropagation();
        setLampState(true, true);
      });
    }

    if (pullChain) {
      pullChain.addEventListener("click", (e) => {
        e.stopPropagation();
        setLampState(!isLampLit, true);
      });
    }

    if (lampHead) {
      lampHead.addEventListener("click", (e) => {
        e.stopPropagation();
        setLampState(!isLampLit, true);
      });
    }
  })();

  /* ========================================================
     MANDATORY GITHUB AUTO-UPDATE & VERSION GATE
     Checks GitHub repository for new releases.
     If outdated: automatically disables legacy interface and
     displays update prompt. Only visible when update is available.
     ======================================================== */
  const UpdateManager = (() => {
    const CURRENT_VERSION = "2.1.0";
    const GITHUB_REPO_OWNER = "tanjeem180hz";
    const GITHUB_REPO_NAME = "180hz_video-downloader";
    const GITHUB_VERSION_URL = `https://raw.githubusercontent.com/${GITHUB_REPO_OWNER}/${GITHUB_REPO_NAME}/main/version.json`;
    const GITHUB_PAGES_URL = `https://${GITHUB_REPO_OWNER}.github.io/${GITHUB_REPO_NAME}/version.json`;
    const GITHUB_SETUP_URL = `https://raw.githubusercontent.com/${GITHUB_REPO_OWNER}/${GITHUB_REPO_NAME}/main/180Hz%20Downloader%20Setup.exe`;
    const GITHUB_PAGE_URL = `https://github.com/${GITHUB_REPO_OWNER}/${GITHUB_REPO_NAME}`;

    let isUpdateAvailable = false;
    let updateData = null;
    let isUpdating = false;

    function parseSemver(v) {
      if (!v) return [0, 0, 0];
      const clean = String(v).trim().replace(/^v/i, "");
      return clean.split(".").map((n) => parseInt(n, 10) || 0);
    }

    function isNewer(remote, local) {
      const r = parseSemver(remote);
      const l = parseSemver(local);
      const max = Math.max(r.length, l.length);
      for (let i = 0; i < max; i++) {
        const rv = r[i] || 0;
        const lv = l[i] || 0;
        if (rv > lv) return true;
        if (rv < lv) return false;
      }
      return false;
    }

    async function checkGithubVersion() {
      // 1. Try local backend check endpoint first
      try {
        const res = await fetch(`${BACKEND_API_BASE}/system/check-update`, { cache: "no-store" });
        if (res.ok) {
          const data = await res.json();
          if (data && data.success && data.latestVersion) {
            handleVersionCheckResult(data.latestVersion, data);
            return;
          }
        }
      } catch {}

      // 2. Fetch directly from GitHub raw URL with timestamp cache-buster
      const ts = Date.now();
      try {
        const res = await fetch(`${GITHUB_VERSION_URL}?_=${ts}`, { cache: "no-store" });
        if (res.ok) {
          const data = await res.json();
          if (data && data.version) {
            handleVersionCheckResult(data.version, data);
            return;
          }
        }
      } catch {}

      // 3. Fallback to GitHub Pages
      try {
        const res = await fetch(`${GITHUB_PAGES_URL}?_=${ts}`, { cache: "no-store" });
        if (res.ok) {
          const data = await res.json();
          if (data && data.version) {
            handleVersionCheckResult(data.version, data);
            return;
          }
        }
      } catch {}
    }

    function handleVersionCheckResult(remoteVersion, fullData) {
      if (isNewer(remoteVersion, CURRENT_VERSION)) {
        isUpdateAvailable = true;
        updateData = fullData || { version: remoteVersion };
        applyUpdateGate(remoteVersion, updateData);
      } else {
        isUpdateAvailable = false;
        removeUpdateGate();
      }
    }

    function applyUpdateGate(latestVersion, data) {
      // 1. Lock the application
      document.body.classList.add("version-locked");

      const urlInp = document.getElementById("urlInput");
      if (urlInp) {
        urlInp.disabled = true;
        urlInp.placeholder = "⚠️ VERSION DEPRECATED — Please click UPDATE NOW above to continue";
      }

      const anBtn = document.getElementById("analyzeBtn");
      if (anBtn) anBtn.disabled = true;

      // 2. Populate version numbers
      const gateCard = document.getElementById("updateGateCard");
      const navBtn = document.getElementById("navUpdateBtn");
      const gateCurrent = document.getElementById("gateCurrentVer");
      const gateLatest = document.getElementById("gateLatestVer");
      const navVer = document.getElementById("navUpdateVer");
      const changelog = document.getElementById("gateChangelog");

      if (gateCurrent) gateCurrent.textContent = `v${CURRENT_VERSION}`;
      if (gateLatest) gateLatest.textContent = `v${latestVersion}`;
      if (navVer) navVer.textContent = `v${latestVersion}`;
      if (changelog && data && data.changelog) {
        changelog.textContent = `Release notes: ${data.changelog}`;
      }

      // 3. Show Update button & gate card (MUST only show when update exists)
      if (gateCard) gateCard.classList.remove("hidden");
      if (navBtn) navBtn.classList.remove("hidden");
    }

    function removeUpdateGate() {
      document.body.classList.remove("version-locked");

      const urlInp = document.getElementById("urlInput");
      if (urlInp && urlInp.placeholder.includes("VERSION DEPRECATED")) {
        urlInp.disabled = false;
        urlInp.placeholder = "Paste YouTube, Facebook, or Instagram link…";
      }

      const anBtn = document.getElementById("analyzeBtn");
      if (anBtn) anBtn.disabled = false;

      const gateCard = document.getElementById("updateGateCard");
      const navBtn = document.getElementById("navUpdateBtn");
      if (gateCard) gateCard.classList.add("hidden");
      if (navBtn) navBtn.classList.add("hidden");
    }

    async function executeUpdate() {
      if (isUpdating) return;
      isUpdating = true;

      const btn = document.getElementById("gateUpdateBtn");
      const btnText = document.getElementById("gateUpdateBtnText");
      const navBtn = document.getElementById("navUpdateBtn");

      if (btn) btn.disabled = true;
      if (btnText) btnText.innerHTML = `<span class="spin-ring"></span> DOWNLOADING UPDATE FROM GITHUB...`;
      if (navBtn) navBtn.disabled = true;

      // Try backend automatic installer application first
      try {
        const res = await fetch(`${BACKEND_API_BASE}/system/apply-update`, {
          method: "POST",
          headers: { "Content-Type": "application/json" }
        });
        const data = await res.json();
        if (data && data.success) {
          if (btnText) btnText.textContent = "LAUNCHING INSTALLER & RESTARTING...";
          return;
        }
      } catch {}

      // Fallback: download setup directly or open GitHub release in browser
      const dlUrl = (updateData && updateData.downloadUrl) || GITHUB_SETUP_URL;
      try {
        const a = document.createElement("a");
        a.href = dlUrl;
        a.download = "180Hz Downloader Setup.exe";
        a.target = "_blank";
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        if (btnText) btnText.textContent = "DOWNLOAD STARTED — RUN SETUP TO COMPLETE";
      } catch {
        window.open(GITHUB_PAGE_URL, "_blank");
      }

      setTimeout(() => {
        isUpdating = false;
        if (btn) btn.disabled = false;
        if (btnText) btnText.textContent = "UPDATE NOW & RESTART";
        if (navBtn) navBtn.disabled = false;
      }, 8000);
    }

    function init() {
      const gateBtn = document.getElementById("gateUpdateBtn");
      if (gateBtn) {
        gateBtn.addEventListener("click", (e) => {
          e.preventDefault();
          executeUpdate();
        });
      }

      const navBtn = document.getElementById("navUpdateBtn");
      if (navBtn) {
        navBtn.addEventListener("click", (e) => {
          e.preventDefault();
          executeUpdate();
        });
      }

      // Check version immediately on launch
      checkGithubVersion();

      // Re-check periodically every 45 seconds
      setInterval(checkGithubVersion, 45000);

      // Re-check when window regains focus
      window.addEventListener("focus", checkGithubVersion, { passive: true });
    }

    return { init, checkGithubVersion, executeUpdate, CURRENT_VERSION };
  })();

  // Initialize Update Manager
  UpdateManager.init();
})();
