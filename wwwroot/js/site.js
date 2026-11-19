// QuickDrop site scripts - lightweight enhancements

// Smooth scroll for anchor links (progressive enhancement)
(function(){
  if ('scrollBehavior' in document.documentElement.style) {
    document.querySelectorAll('a[href^="#"]').forEach(function(a){
      a.addEventListener('click', function(e){
        var id = this.getAttribute('href');
        if (id && id.length>1){
          var el = document.querySelector(id);
          if (el){ e.preventDefault(); el.scrollIntoView({behavior:'smooth'}); }
        }
      });
    });
  }
})();

// Admin revenue chart initializer (safe)
(function(){
  if (!document.getElementById('revenueChart')) return;
  try{
    var ctx = document.getElementById('revenueChart').getContext('2d');
    var labels = ['Mon','Tue','Wed','Thu','Fri','Sat','Sun'];
    var data = { labels: labels, datasets: [{ label: 'Revenue', data: [120000,150000,90000,200000,180000,220000,160000], backgroundColor: 'rgba(108,43,217,0.12)', borderColor: 'rgba(108,43,217,0.9)', tension: 0.3, fill: true }] };
    new Chart(ctx, {
      type: 'line',
      data: data,
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: { ticks: { color:'#B8AFC5' }, grid: { color:'rgba(255,255,255,0.03)' } },
          x: { ticks: { color:'#B8AFC5' }, grid: { color:'rgba(255,255,255,0.02)' } }
        }
      }
    });
  }catch(e){ console.warn('Chart render failed', e); }
})();

// Login page interactions: toggle password and loading state
(function(){
  try{
    var toggle = document.getElementById('togglePwd');
    var pwd = document.getElementById('passwordInput');
    var form = document.getElementById('loginForm');
    var card = document.querySelector('.login-card');
    if (toggle && pwd){
      toggle.addEventListener('click', function(){
        var show = pwd.type === 'password';
        pwd.type = show ? 'text' : 'password';
        toggle.textContent = show ? 'Hide' : 'Show';
        toggle.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
        toggle.setAttribute('aria-pressed', show ? 'true' : 'false');
      });
    }
    if (form && card){
      form.addEventListener('submit', function(){
        card.classList.add('loading');
        var btn = form.querySelector('button[type="submit"]');
        if (btn){ btn.disabled = true; btn.textContent = 'Signing in...'; }
      });
    }
  }catch(e){}
})();

// Reveal on scroll (for .reveal, .slide-up etc.)
(function(){
  try{
    var els = document.querySelectorAll('.reveal, .slide-up');
    if (!('IntersectionObserver' in window) || !els.length) {
      els.forEach(function(e){ e.classList.add('in-view'); });
      return;
    }
    var obs = new IntersectionObserver(function(entries){
      entries.forEach(function(entry){
        if (entry.isIntersecting) {
          entry.target.classList.add('in-view');
          obs.unobserve(entry.target);
        }
      });
    }, {threshold:0.12});
    els.forEach(function(e){ obs.observe(e); });
  }catch(e){}
})();

// Order tracker initializer: mark steps active/completed based on data-status
(function(){
  try{
    function mapStatusToStep(status){
      if (!status) return 1;
      status = status.toLowerCase();
      if (status.includes('deliv')) return 5;
      if (status.includes('way') || status.includes('on the way')) return 4;
      if (status.includes('pick') || status.includes('picked')) return 3;
      if (status.includes('prepar') || status.includes('preparing')) return 2;
      return status == 'pending' ? 1 : 1;
    }

    document.querySelectorAll('.order-tracker').forEach(function(tr){
      var status = tr.getAttribute('data-status') || '';
      var step = mapStatusToStep(status);
      tr.querySelectorAll('.step').forEach(function(s){
        var sidx = parseInt(s.getAttribute('data-step')) || 0;
        if (sidx < step) s.classList.add('completed');
        if (sidx === step) s.classList.add('active');
      });
    });
  }catch(e){}
})();

// Category filter on restaurant page
(function(){
  try{
    var links = document.querySelectorAll('.category-link');
    if (!links.length) return;
    function setActive(link){ links.forEach(function(l){ l.classList.remove('active'); }); link.classList.add('active'); }
    links.forEach(function(link, idx){
      link.addEventListener('click', function(e){
        e.preventDefault();
        var cat = link.getAttribute('data-cat');
        setActive(link);
        document.querySelectorAll('.product-item').forEach(function(item){
          if (cat === 'Popular'){ item.style.display = ''; }
          else { item.style.display = item.getAttribute('data-category') === cat ? '' : 'none'; }
        });
      });
      if (idx===0) link.classList.add('active');
    });
  }catch(e){}
})();

// Mobile menu toggle: toggle 'open' class and update aria-expanded
(function(){
  var btn = document.getElementById('mobileToggle');
  var menu = document.getElementById('mobileMenu');
  if (!btn || !menu) return;

  function setMenuOpen(open){
    menu.classList.toggle('open', open);
    btn.setAttribute('aria-expanded', String(open));
    btn.setAttribute('aria-label', open ? 'Close menu' : 'Open menu');
  }

  btn.addEventListener('click', function(){
    setMenuOpen(btn.getAttribute('aria-expanded') !== 'true');
  });
  menu.querySelectorAll('a').forEach(function(link){
    link.addEventListener('click', function(){ setMenuOpen(false); });
  });
  document.addEventListener('keydown', function(event){
    if (event.key === 'Escape') setMenuOpen(false);
  });
  window.addEventListener('resize', function(){
    if (window.innerWidth > 991) setMenuOpen(false);
  });
})();

// Page load fade-in and smooth page transitions
(function(){
  try{
    function onReady(){ document.body.classList.add('page-loaded'); }
    if (document.readyState === 'complete' || document.readyState === 'interactive') onReady(); else document.addEventListener('DOMContentLoaded', onReady);
    // Intercept in-page anchor clicks for smooth scrolling
    document.addEventListener('click', function(e){ var a = e.target.closest('a'); if (!a) return; var href = a.getAttribute('href'); if (!href || href.indexOf('#')!==0) return; var id = href.substring(1); var el = document.getElementById(id); if (el){ e.preventDefault(); el.scrollIntoView({behavior:'smooth'}); } });
  }catch(e){}
})();

// Animate numeric counters when visible
(function(){
  try{
    var observer = null;
    var els = document.querySelectorAll('.stat-number[data-count]');
    if ('IntersectionObserver' in window && els.length){
      observer = new IntersectionObserver(function(entries){
        entries.forEach(function(entry){
          if (!entry.isIntersecting) return;
          var el = entry.target; if (el.getAttribute('data-animated')) return; el.setAttribute('data-animated','1');
          var raw = el.getAttribute('data-count'); var isCurrency = String(raw).toLowerCase().indexOf('ugx')>=0;
          var target = isCurrency ? null : (Number(raw)||0); var duration = 900; var startTs = null;
          function step(ts){ if (!startTs) startTs = ts; var progress = Math.min((ts-startTs)/duration,1); var value = Math.floor(progress*target); el.textContent = isCurrency ? raw : value; if (progress<1) requestAnimationFrame(step); }
          if (!isCurrency) requestAnimationFrame(step); else el.textContent = raw;
          observer.unobserve(el);
        });
      }, {threshold:0.2});
      els.forEach(function(e){ observer.observe(e); });
    } else {
      els.forEach(function(el){ if (!el.getAttribute('data-animated')){ el.textContent = el.getAttribute('data-count'); el.setAttribute('data-animated','1'); } });
    }
  }catch(e){}
})();

// Initialize progress bars (elements with .progress-bar and child <i data-progress>)
(function(){
  try{
    document.querySelectorAll('.progress-bar').forEach(function(pb){
      var bar = pb.querySelector('i'); if (!bar) return; var progress = bar.getAttribute('data-progress') || bar.style.width || '0%'; setTimeout(function(){ bar.style.width = progress; }, 120);
    });
  }catch(e){}
})();

// Toast notifications API
(function(){
  try{
    var container = document.getElementById('toastContainer');
    if (!container){ container = document.createElement('div'); container.id = 'toastContainer'; document.body.appendChild(container); }
    window.showToast = function(message, type, timeout){
      timeout = timeout || 4200; type = type || 'success';
      var t = document.createElement('div'); t.className = 'toast-item ' + type; t.innerHTML = '<div style="flex:1">' + message + '</div><div style="margin-left:8px;opacity:0.8;cursor:pointer" data-dismiss aria-label="dismiss">✕</div>';
      var dismiss = t.querySelector('[data-dismiss]'); dismiss.addEventListener('click', function(){ hide(t); });
      container.appendChild(t); setTimeout(function(){ t.classList.add('show'); }, 10);
      var id = setTimeout(function(){ hide(t); }, timeout);
      function hide(el){ if (!el) return; el.classList.remove('show'); setTimeout(function(){ try{ container.removeChild(el);}catch(e){} }, 320); clearTimeout(id); }
      return t;
    };
  }catch(e){}
})();

// Expose small debugging helper
window.qd = window.qd || {};
window.qd.showToast = window.showToast || function(){ console.warn('showToast unavailable'); };

// Shared image fallback for pages that render database-provided URLs.
(function(){
  try{
    var fallback = '/images/food-placeholder.jpg';
    document.querySelectorAll('img').forEach(function(image){
      image.addEventListener('error', function(){
        if (image.dataset.fallbackApplied === 'true') return;
        image.dataset.fallbackApplied = 'true';
        image.src = image.dataset.fallback || fallback;
      }, {once:false});
    });
  }catch(e){}
})();

// Avoid reveal transforms when reduced motion is requested.
(function(){
  try{
    if (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      document.querySelectorAll('.reveal, .slide-up').forEach(function(element){ element.classList.add('in-view'); });
    }
  }catch(e){}
})();
