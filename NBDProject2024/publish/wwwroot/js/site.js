document.addEventListener("DOMContentLoaded", function () {
  var themeStorageKey = "nbd-theme";
  var sidebarThemeStorageKey = "nbd-sidebar-theme";
  var topbarThemeStorageKey = "nbd-topbar-theme";
  var themeToggleBtn = document.querySelector("#themeToggleBtn");
  var themeDarkToggle = document.querySelector("#themeDarkToggle");
  var themeSystemCheckbox = document.querySelector("#themeSystemCheckbox");
  var themeModeText = document.querySelector("#themeModeText");
  var sidebarDarkToggle = document.querySelector("#sidebarDarkToggle");
  var topbarDarkToggle = document.querySelector("#topbarDarkToggle");
  var sidebarThemeText = document.querySelector("#sidebarThemeText");
  var topbarThemeText = document.querySelector("#topbarThemeText");
  var copyEmailBtn = document.querySelector("#copyEmailBtn");
  var fullscreenToggleBtn = document.querySelector("#fullscreenToggleBtn");
  var fullscreenIntentStorageKey = "nbd-fullscreen-intent";
  var prefersDarkQuery = window.matchMedia("(prefers-color-scheme: dark)");
  var themeModes = ["light", "dark", "system"];
  var isNavigatingAway = false;

  function isFullscreenActive() {
    return !!(
      document.fullscreenElement ||
      document.webkitFullscreenElement ||
      document.msFullscreenElement
    );
  }

  function hasFullscreenIntent() {
    return sessionStorage.getItem(fullscreenIntentStorageKey) === "1";
  }

  function setFullscreenIntent(enabled) {
    if (enabled) {
      sessionStorage.setItem(fullscreenIntentStorageKey, "1");
    } else {
      sessionStorage.removeItem(fullscreenIntentStorageKey);
    }
  }

  async function requestFullscreenMode() {
    if (!document.documentElement) {
      return;
    }

    if (document.documentElement.requestFullscreen) {
      await document.documentElement.requestFullscreen();
    } else if (document.documentElement.webkitRequestFullscreen) {
      document.documentElement.webkitRequestFullscreen();
    } else if (document.documentElement.msRequestFullscreen) {
      document.documentElement.msRequestFullscreen();
    }
  }

  function updateFullscreenButtonState() {
    if (!fullscreenToggleBtn) {
      return;
    }

    var active = isFullscreenActive();
    fullscreenToggleBtn.innerHTML = active
      ? '<i class="fa-solid fa-compress"></i>'
      : '<i class="fa-solid fa-expand"></i>';
    fullscreenToggleBtn.setAttribute(
      "title",
      active ? "Exit fullscreen" : "Enter fullscreen",
    );
    fullscreenToggleBtn.setAttribute("aria-label", fullscreenToggleBtn.title);
  }

  async function toggleFullscreen() {
    if (!document.documentElement) {
      return;
    }

    try {
      if (!isFullscreenActive()) {
        setFullscreenIntent(true);
        await requestFullscreenMode();
      } else if (document.exitFullscreen) {
        setFullscreenIntent(false);
        await document.exitFullscreen();
      } else if (document.webkitExitFullscreen) {
        setFullscreenIntent(false);
        document.webkitExitFullscreen();
      } else if (document.msExitFullscreen) {
        setFullscreenIntent(false);
        document.msExitFullscreen();
      }
    } catch (error) {
      // Ignore fullscreen errors caused by browser policy restrictions.
    }
  }

  async function restoreFullscreenIfNeeded() {
    if (!hasFullscreenIntent() || isFullscreenActive()) {
      return;
    }

    try {
      await requestFullscreenMode();
    } catch (error) {
      // Ignore restore failures due to browser activation policies.
    }
  }

  function getSavedThemeMode() {
    var saved = localStorage.getItem(themeStorageKey);
    return saved === "light" || saved === "dark" || saved === "system"
      ? saved
      : null;
  }

  function getSystemTheme() {
    return prefersDarkQuery.matches ? "dark" : "light";
  }

  function getSavedBarTheme(storageKey) {
    var saved = localStorage.getItem(storageKey);
    return saved === "light" || saved === "dark" ? saved : null;
  }

  function clearSavedBarThemes() {
    localStorage.removeItem(sidebarThemeStorageKey);
    localStorage.removeItem(topbarThemeStorageKey);
  }

  function applyBarThemes(sidebarTheme, topbarTheme) {
    document.documentElement.setAttribute("data-sidebar-theme", sidebarTheme);
    document.documentElement.setAttribute("data-topbar-theme", topbarTheme);

    if (sidebarDarkToggle) {
      sidebarDarkToggle.checked = sidebarTheme === "dark";
    }

    if (topbarDarkToggle) {
      topbarDarkToggle.checked = topbarTheme === "dark";
    }

    if (sidebarThemeText) {
      sidebarThemeText.textContent =
        "Current sidebar mode: " + getModeLabel(sidebarTheme);
    }

    if (topbarThemeText) {
      topbarThemeText.textContent =
        "Current topbar mode: " + getModeLabel(topbarTheme);
    }
  }

  function applyGlobalMode(nextMode, options) {
    var resolvedTheme = getResolvedTheme(nextMode);
    var syncBars = !options || options.syncBars !== false;

    localStorage.setItem(themeStorageKey, nextMode);
    applyTheme(resolvedTheme, nextMode);

    if (syncBars) {
      clearSavedBarThemes();
      applyBarThemes(resolvedTheme, resolvedTheme);
    }
  }

  function getResolvedTheme(mode) {
    return mode === "system" ? getSystemTheme() : mode;
  }

  function getNextMode(currentMode) {
    var currentIndex = themeModes.indexOf(currentMode);
    var nextIndex = (currentIndex + 1) % themeModes.length;
    return themeModes[nextIndex];
  }

  function getModeLabel(mode) {
    if (mode === "light") {
      return "Light";
    }
    if (mode === "dark") {
      return "Dark";
    }
    return "System";
  }

  function getModeIcon(mode) {
    if (mode === "light") {
      return '<i class="fa-regular fa-sun"></i>';
    }
    if (mode === "dark") {
      return '<i class="fa-regular fa-moon"></i>';
    }
    return '<i class="fa-solid fa-circle-half-stroke"></i>';
  }

  function applyTheme(theme, mode) {
    document.documentElement.setAttribute("data-theme", theme);
    document.documentElement.setAttribute("data-theme-mode", mode);

    if (themeToggleBtn) {
      var nextMode = getNextMode(mode);
      themeToggleBtn.innerHTML = getModeIcon(mode);
      themeToggleBtn.setAttribute(
        "title",
        "Theme: " +
          getModeLabel(mode) +
          " (click to switch to " +
          getModeLabel(nextMode) +
          ")",
      );
      themeToggleBtn.setAttribute("aria-label", themeToggleBtn.title);
    }

    if (themeSystemCheckbox) {
      themeSystemCheckbox.checked = mode === "system";
    }

    if (themeDarkToggle) {
      themeDarkToggle.checked = theme === "dark";
      themeDarkToggle.disabled = mode === "system";
    }

    if (themeModeText) {
      var label =
        mode === "system"
          ? "System (currently " + getModeLabel(theme) + ")"
          : getModeLabel(mode);
      themeModeText.textContent = "Current mode: " + label;
    }

    document.dispatchEvent(
      new CustomEvent("nbd:themeChanged", {
        detail: { theme: theme, mode: mode },
      }),
    );
  }

  var activeMode = getSavedThemeMode() || "system";
  var activeTheme = getResolvedTheme(activeMode);
  applyTheme(activeTheme, activeMode);

  var activeSidebarTheme =
    getSavedBarTheme(sidebarThemeStorageKey) || activeTheme;
  var activeTopbarTheme =
    getSavedBarTheme(topbarThemeStorageKey) || activeTheme;
  applyBarThemes(activeSidebarTheme, activeTopbarTheme);

  if (fullscreenToggleBtn) {
    updateFullscreenButtonState();
    fullscreenToggleBtn.addEventListener("click", function () {
      toggleFullscreen();
    });
    document.addEventListener("fullscreenchange", function () {
      updateFullscreenButtonState();

      // If fullscreen was closed manually (Esc/browser UI), clear sticky intent.
      if (!isFullscreenActive() && !isNavigatingAway) {
        setFullscreenIntent(false);
      }
    });
    document.addEventListener(
      "webkitfullscreenchange",
      updateFullscreenButtonState,
    );
    document.addEventListener(
      "MSFullscreenChange",
      updateFullscreenButtonState,
    );

    window.addEventListener("beforeunload", function () {
      if (hasFullscreenIntent()) {
        isNavigatingAway = true;
      }
    });

    // Attempt to restore fullscreen after internal navigation when sticky mode is enabled.
    restoreFullscreenIfNeeded();
  }

  if (themeToggleBtn) {
    themeToggleBtn.addEventListener("click", function () {
      var currentMode =
        document.documentElement.getAttribute("data-theme-mode") || "system";
      var nextMode = getNextMode(currentMode);
      applyGlobalMode(nextMode, { syncBars: true });
    });
  }

  if (themeSystemCheckbox) {
    themeSystemCheckbox.addEventListener("change", function () {
      var nextMode = themeSystemCheckbox.checked
        ? "system"
        : themeDarkToggle && themeDarkToggle.checked
          ? "dark"
          : "light";
      applyGlobalMode(nextMode, { syncBars: true });
    });
  }

  if (themeDarkToggle) {
    themeDarkToggle.addEventListener("change", function () {
      var currentMode =
        document.documentElement.getAttribute("data-theme-mode") || "system";
      if (currentMode === "system") {
        return;
      }

      var nextMode = themeDarkToggle.checked ? "dark" : "light";
      applyGlobalMode(nextMode, { syncBars: true });
    });
  }

  if (sidebarDarkToggle) {
    sidebarDarkToggle.addEventListener("change", function () {
      var nextTheme = sidebarDarkToggle.checked ? "dark" : "light";
      localStorage.setItem(sidebarThemeStorageKey, nextTheme);
      applyBarThemes(
        nextTheme,
        document.documentElement.getAttribute("data-topbar-theme") || "light",
      );
    });
  }

  if (topbarDarkToggle) {
    topbarDarkToggle.addEventListener("change", function () {
      var nextTheme = topbarDarkToggle.checked ? "dark" : "light";
      localStorage.setItem(topbarThemeStorageKey, nextTheme);
      applyBarThemes(
        document.documentElement.getAttribute("data-sidebar-theme") || "light",
        nextTheme,
      );
    });
  }

  function handleSystemThemeChange(event) {
    var currentMode =
      document.documentElement.getAttribute("data-theme-mode") || "system";
    if (currentMode === "system") {
      applyTheme(event.matches ? "dark" : "light", "system");

      var hasSidebarOverride = !!getSavedBarTheme(sidebarThemeStorageKey);
      var hasTopbarOverride = !!getSavedBarTheme(topbarThemeStorageKey);
      if (!hasSidebarOverride && !hasTopbarOverride) {
        var systemTheme = event.matches ? "dark" : "light";
        applyBarThemes(systemTheme, systemTheme);
      }
    }
  }

  if (typeof prefersDarkQuery.addEventListener === "function") {
    prefersDarkQuery.addEventListener("change", handleSystemThemeChange);
  } else if (typeof prefersDarkQuery.addListener === "function") {
    prefersDarkQuery.addListener(handleSystemThemeChange);
  }

  if (copyEmailBtn) {
    copyEmailBtn.addEventListener("click", function () {
      var email = copyEmailBtn.getAttribute("data-email") || "";
      if (!email) {
        return;
      }

      var resetButtonLabel = function () {
        copyEmailBtn.textContent = "Copy";
      };

      if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(email).then(function () {
          copyEmailBtn.textContent = "Copied";
          window.setTimeout(resetButtonLabel, 1400);
        });
      } else {
        var fallbackInput = document.createElement("input");
        fallbackInput.value = email;
        document.body.appendChild(fallbackInput);
        fallbackInput.select();
        document.execCommand("copy");
        document.body.removeChild(fallbackInput);
        copyEmailBtn.textContent = "Copied";
        window.setTimeout(resetButtonLabel, 1400);
      }
    });
  }

  var tables = document.querySelectorAll("table:not(.no-auto-responsive)");
  tables.forEach(function (table) {
    if (table.closest(".table-responsive")) {
      return;
    }

    var wrapper = document.createElement("div");
    wrapper.className = "table-responsive mb-3";
    table.parentNode.insertBefore(wrapper, table);
    wrapper.appendChild(table);
  });

  var wideElements = document.querySelectorAll("pre, code, .input-group");
  wideElements.forEach(function (element) {
    element.style.maxWidth = "100%";
  });
});
