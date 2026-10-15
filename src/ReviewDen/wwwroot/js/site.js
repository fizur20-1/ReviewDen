const motionOn = () => !matchMedia('(prefers-reduced-motion: reduce)').matches;

// Typing headline: types each category name, pauses, deletes, moves on.
document.querySelectorAll('.typed').forEach(el => {
    const words = el.dataset.words.split('|').filter(Boolean);
    let w = 0, n = words[0].length, deleting = true;
    const tick = () => {
        if (!motionOn()) { el.textContent = words[w]; n = words[w].length; deleting = true; return setTimeout(tick, 400); }
        const word = words[w];
        n += deleting ? -1 : 1;
        el.textContent = word.slice(0, n);
        let wait = deleting ? 45 : 95;
        if (!deleting && n >= word.length) { deleting = true; wait = 1600; }
        else if (deleting && n <= 0) { deleting = false; w = (w + 1) % words.length; n = 0; wait = 300; }
        setTimeout(tick, wait);
    };
    setTimeout(tick, 1800);
});

// Fade cards and comments up as they scroll into view.
const io = 'IntersectionObserver' in window && new IntersectionObserver(entries => entries.forEach(e => {
    if (e.isIntersecting) { e.target.classList.add('in'); io.unobserve(e.target); }
}), { rootMargin: '0px 0px -40px 0px' });
document.querySelectorAll('.reveal').forEach(el => io ? io.observe(el) : el.classList.add('in'));

// Shimmer placeholder until each photo has loaded.
document.querySelectorAll('.shimmer img').forEach(img => {
    const done = () => img.parentElement.classList.add('loaded');
    img.complete ? done() : img.addEventListener('load', done, { once: true });
    img.addEventListener('error', done, { once: true });
});

// Like: pop the button and burst hearts, then submit.
document.querySelectorAll('form[data-like]').forEach(form => form.addEventListener('submit', e => {
    if (!motionOn() || form.dataset.sent) return;
    e.preventDefault();
    form.dataset.sent = '1';
    const btn = form.querySelector('button');
    btn.classList.add('like-pop');
    btn.querySelector('i').className = 'bi bi-heart-fill';
    btn.querySelector('span').textContent = +btn.querySelector('span').textContent + 1;
    const r = btn.getBoundingClientRect();
    for (let k = 0; k < 7; k++) {
        const h = Object.assign(document.createElement('img'), {
            src: 'https://fonts.gstatic.com/s/e/notoemoji/latest/2764_fe0f/512.webp', className: 'burst', width: 26 + Math.random() * 14, alt: ''
        });
        h.style.left = r.left + r.width / 2 + 'px';
        h.style.top = r.top + r.height / 2 + 'px';
        h.style.setProperty('--dx', (Math.random() * 140 - 70) + 'px');
        h.style.setProperty('--rot', (Math.random() * 60 - 30) + 'deg');
        h.style.animationDelay = k * 40 + 'ms';
        document.body.append(h);
        setTimeout(() => h.remove(), 1200);
    }
    setTimeout(() => form.submit(), 650);
}));

// Resize every picked photo to <=1600px JPEG in the browser: smaller uploads, EXIF dropped, server only has to accept JPEG.
const photo = document.getElementById('Photo');
if (photo) {
    photo.addEventListener('change', async () => {
        const file = photo.files[0];
        if (!file) return;
        try {
            const bmp = await createImageBitmap(file);
            const scale = Math.min(1, 1600 / Math.max(bmp.width, bmp.height));
            const c = Object.assign(document.createElement('canvas'), { width: Math.round(bmp.width * scale), height: Math.round(bmp.height * scale) });
            c.getContext('2d').drawImage(bmp, 0, 0, c.width, c.height);
            const blob = await new Promise(r => c.toBlob(r, 'image/jpeg', 0.85));
            const dt = new DataTransfer();
            dt.items.add(new File([blob], 'photo.jpg', { type: 'image/jpeg' }));
            photo.files = dt.files;
            const prev = document.getElementById('photo-preview');
            prev.src = URL.createObjectURL(blob);
            prev.hidden = false;
            document.getElementById('photo-hint').hidden = true;
        } catch {
            photo.value = '';
            alert('Could not read this photo. Try a JPG or PNG.');
        }
    });
}

// Remember the poster's name between visits.
document.querySelectorAll('[data-remember]').forEach(el => {
    const key = 'rd-' + el.dataset.remember;
    try { if (!el.value) el.value = localStorage.getItem(key) || ''; } catch { }
    el.form.addEventListener('submit', () => { try { localStorage.setItem(key, el.value); } catch { } });
});

// Mood effects: every review page is themed by the reviewer's verdict (love, good, okay, bad, scam).
const MOODS = {
    love: { move: 'rise', emojis: ['💖', '💕', '😍', '💗'], n: 22, intro: 'confetti' },
    good: { move: 'rise', emojis: ['👍', '✨', '💚', '⭐'], n: 16, intro: 'sparkle' },
    okay: { move: 'drift', emojis: ['😐', '🤔', '🙂'], n: 10 },
    bad: { move: 'fall', emojis: ['👎', '😞', '💔'], n: 12, intro: 'sink' },
    scam: { move: 'fall', emojis: ['🚨', '⚠️', '💸', '🤬'], n: 20, intro: 'alarm' },
};
const rnd = (a, b) => a + Math.random() * (b - a);

function moodFx(layer, key, extra = []) {
    const m = MOODS[key];
    layer.replaceChildren();
    if (!m || !motionOn()) return;
    const pool = [...m.emojis, ...m.emojis, ...extra];
    const count = innerWidth < 600 ? Math.round(m.n * .65) : m.n;
    for (let k = 0; k < count; k++) {
        const p = document.createElement('span');
        p.className = 'p ' + m.move;
        p.textContent = pool[k % pool.length];
        const d = m.move === 'drift' ? rnd(4, 8) : rnd(7, 15);
        p.style.cssText = `left:${rnd(0, 96)}vw;font-size:${rnd(18, 42)}px;--d:${d}s;--delay:${-rnd(0, d)}s;--sway:${rnd(-12, 12)}vw;--spin:${rnd(-200, 200)}deg;--y:${rnd(5, 85)}vh`;
        layer.append(p);
    }
}

function moodIntro(key, color) {
    const intro = MOODS[key]?.intro;
    if (!intro || !motionOn()) return;
    if (intro === 'alarm') {
        document.querySelector('main').classList.add('shake');
        document.body.append(Object.assign(document.createElement('div'), { className: 'flash' }));
        setTimeout(() => document.querySelector('.flash')?.remove(), 800);
        return;
    }
    if (intro === 'sink') return;
    const pieces = intro === 'confetti' ? ['💖', '🎉', '✨', '💕', '■', '●'] : ['✨', '⭐', '👍', '●'];
    const colors = [color, '#ffd23f', '#3fc5ff', '#ff7ab6', '#7cff9b'];
    const cx = innerWidth / 2, cy = innerHeight * .35;
    for (let k = 0; k < (intro === 'confetti' ? 60 : 30); k++) {
        const s = document.createElement('span');
        s.className = 'confetti';
        s.textContent = pieces[k % pieces.length];
        const angle = rnd(0, Math.PI * 2), dist = rnd(120, Math.max(innerWidth, innerHeight) * .6);
        s.style.cssText = `color:${colors[k % colors.length]};font-size:${rnd(14, 30)}px;--d:${rnd(1.2, 2.2)}s;--spin:${rnd(-540, 540)}deg;` +
            `--x0:${cx}px;--y0:${cy}px;--x1:${cx + Math.cos(angle) * dist}px;--y1:${cy + Math.sin(angle) * dist + 200}px`;
        document.body.append(s);
        setTimeout(() => s.remove(), 2400);
    }
}

document.querySelectorAll('.fx').forEach(layer => {
    document.body.prepend(layer);
    const key = document.body.dataset.mood;
    moodFx(layer, key, (layer.dataset.emojis || '').split(' ').filter(Boolean));
    moodIntro(key, getComputedStyle(document.body).getPropertyValue('--mood'));
});

// Post form: picking a mood previews that mood's theme on the whole page.
document.querySelectorAll('input[name="Mood"]').forEach(r => r.addEventListener('change', () => {
    let layer = document.querySelector('.fx');
    if (!layer) document.body.prepend(layer = Object.assign(document.createElement('div'), { className: 'fx' }));
    const color = getComputedStyle(r.nextElementSibling).getPropertyValue('--c');
    document.body.dataset.mood = r.dataset.mood;
    document.body.style.setProperty('--mood', color);
    moodFx(layer, r.dataset.mood);
    moodIntro(r.dataset.mood, color);
}));

// Comment box grows with its text, like a chat app.
document.querySelectorAll('[data-autogrow]').forEach(t => {
    const fit = () => { t.style.height = 'auto'; t.style.height = t.scrollHeight + 'px'; };
    t.addEventListener('input', fit);
    fit();
});

// Quick-reply chips drop their text into the comment box.
document.querySelectorAll('[data-quick]').forEach(b => b.addEventListener('click', () => {
    const t = b.closest('form').querySelector('textarea');
    t.value = t.value ? t.value.trimEnd() + ' ' + b.dataset.quick : b.dataset.quick;
    t.dispatchEvent(new Event('input'));
    t.focus();
}));

// Live character counter for the review text.
document.querySelectorAll('[data-count]').forEach(t => {
    const out = document.getElementById(t.dataset.count);
    const show = () => out.textContent = `${t.value.length} / ${t.maxLength}`;
    t.addEventListener('input', show);
    show();
});

// Composer avatar shows the first letter of the commenter's name.
document.querySelectorAll('[data-initial-of]').forEach(a => {
    const input = document.getElementById(a.dataset.initialOf);
    const show = () => { if (input.value.trim()) a.textContent = input.value.trim()[0].toUpperCase(); };
    input.addEventListener('input', show);
    show();
});

// Native share sheet on phones; browsers without it fall back to copying the link.
document.querySelectorAll('[data-share]').forEach(btn => btn.addEventListener('click', async () => {
    if (navigator.share) return navigator.share({ title: document.title, text: btn.dataset.text, url: btn.dataset.url }).catch(() => { });
    await navigator.clipboard.writeText(btn.dataset.url);
    btn.querySelector('span').textContent = btn.dataset.done;
}));

// 1080x1350 portrait card (Facebook/Instagram feed size); phones get the share sheet with the image, desktop downloads it.
document.querySelectorAll('[data-card]').forEach(btn => btn.addEventListener('click', async () => {
    await document.fonts.ready;
    const img = document.getElementById('post-img');
    const d = btn.dataset, W = 1080, H = 1350, P = 60, brand = d.moodColor || '#ff5a1f';
    const c = Object.assign(document.createElement('canvas'), { width: W, height: H });
    const g = c.getContext('2d');
    const font = (size, w = 700) => `${w} ${size}px "Hind Siliguri", system-ui, sans-serif`;
    const iw = img.naturalWidth, ih = img.naturalHeight;

    const cover = Math.max(W / iw, H / ih);
    g.fillStyle = '#222';
    g.fillRect(0, 0, W, H);
    g.filter = 'blur(40px) brightness(0.55)';
    g.drawImage(img, (W - iw * cover) / 2, (H - ih * cover) / 2, iw * cover, ih * cover);
    g.filter = 'none';

    g.font = font(46);
    g.textBaseline = 'middle';
    g.fillStyle = '#fff';
    g.fillText('Review', P, 80);
    g.fillStyle = '#ffa27a';
    g.fillText('Den', P + g.measureText('Review').width, 80);
    pill(g, d.moodChar + ' ' + d.moodLabel, W - P, 80, brand, font(34, 700));

    const panelTop = 880, boxTop = 140, boxH = panelTop - boxTop - 40, boxW = W - P * 2;
    const s = Math.min(boxW / iw, boxH / ih), w = iw * s, h = ih * s, x = (W - w) / 2, y = boxTop + (boxH - h) / 2;
    g.save();
    g.shadowColor = 'rgba(0,0,0,.45)';
    g.shadowBlur = 40;
    g.shadowOffsetY = 12;
    g.beginPath();
    g.roundRect(x, y, w, h, 28);
    g.fillStyle = '#fff';
    g.fill();
    g.shadowColor = 'transparent';
    g.clip();
    g.drawImage(img, x, y, w, h);
    g.restore();
    g.save();
    g.translate(x + w - 30, y + h - 30);
    g.rotate(-.18);
    g.font = '150px serif';
    g.textAlign = 'center';
    g.textBaseline = 'middle';
    g.fillText(d.moodChar, 0, 0);
    g.restore();

    g.fillStyle = '#fff';
    g.beginPath();
    g.roundRect(0, panelTop, W, H - panelTop + 60, [48, 48, 0, 0]);
    g.fill();

    g.fillStyle = brand;
    g.textBaseline = 'alphabetic';
    g.font = font(30, 700);
    g.fillText(d.cat, P, panelTop + 62);
    let ty = panelTop + 128, size = 62, lines;
    do { g.font = font(size); lines = wrap(g, d.title, W - P * 2); } while (lines.length > 2 && (size -= 6) >= 44);
    g.fillStyle = '#161616';
    for (const l of clamp(g, lines, 2, W - P * 2)) { g.fillText(l, P, ty); ty += size * 1.25; }

    g.font = font(32, 400);
    g.fillStyle = '#666';
    ty += 4;
    for (const l of clamp(g, wrap(g, d.body.replace(/\s+/g, ' '), W - P * 2), 2, W - P * 2)) { g.fillText(l, P, ty); ty += 46; }

    const fy = H - 78;
    g.fillStyle = brand;
    g.beginPath();
    g.arc(P + 30, fy, 30, 0, Math.PI * 2);
    g.fill();
    g.fillStyle = '#fff';
    g.font = font(30);
    g.textAlign = 'center';
    g.textBaseline = 'middle';
    g.fillText((d.author[0] || '?').toUpperCase(), P + 30, fy + 2);
    g.textAlign = 'left';
    g.fillStyle = '#222';
    g.font = font(32, 600);
    g.fillText(clamp(g, [d.author], 1, 300)[0] + '  ❤️ ' + d.likes, P + 76, fy + 2);
    pill(g, d.cta + ' → ' + d.site, W - P, fy, brand, font(28, 600));

    const blob = await new Promise(r => c.toBlob(r, 'image/jpeg', 0.92));
    const file = new File([blob], 'reviewden.jpg', { type: 'image/jpeg' });
    if (navigator.canShare?.({ files: [file] }) && matchMedia('(pointer: coarse)').matches) {
        return navigator.share({ files: [file] }).catch(() => { });
    }
    Object.assign(document.createElement('a'), { href: URL.createObjectURL(blob), download: file.name }).click();
}));

function pill(g, text, right, cy, bg, f) {
    g.font = f;
    const ph = 58, pw = g.measureText(text).width + 52, left = right - pw;
    g.fillStyle = bg;
    g.beginPath();
    g.roundRect(left, cy - ph / 2, pw, ph, ph / 2);
    g.fill();
    g.fillStyle = '#fff';
    g.textAlign = 'left';
    g.textBaseline = 'middle';
    g.fillText(text, left + 26, cy + 2);
}

function wrap(g, text, max) {
    const lines = [];
    let line = '';
    for (const word of text.split(/\s+/)) {
        const next = line ? line + ' ' + word : word;
        if (g.measureText(next).width > max && line) { lines.push(line); line = word; } else line = next;
    }
    if (line) lines.push(line);
    return lines;
}

function clamp(g, lines, n, max) {
    if (lines.length <= n && g.measureText(lines[n - 1] ?? '').width <= max) return lines;
    const out = lines.slice(0, n);
    let last = out[n - 1];
    while (last && g.measureText(last + '…').width > max) last = last.slice(0, -1);
    out[n - 1] = last + '…';
    return out;
}
