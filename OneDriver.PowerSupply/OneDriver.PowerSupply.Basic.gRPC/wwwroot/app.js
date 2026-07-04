const state = {
  deviceId: null,
  channels: []
};

const gauges = document.getElementById("gauges");
const gaugeTemplate = document.getElementById("gauge-template");
const connDot = document.getElementById("conn-dot");
const connText = document.getElementById("conn-text");
const messageEl = document.getElementById("message");
const channelSelect = document.getElementById("channel-select");
const voltsInput = document.getElementById("volts-input");
const ampsInput = document.getElementById("amps-input");
const modeSelect = document.getElementById("mode-select");

async function api(path, method = "GET", body = null) {
  const response = await fetch(path, {
    method,
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined
  });

  const payload = await response.json().catch(() => ({}));
  if (!response.ok) {
    throw new Error(payload.error || `Request failed (${response.status})`);
  }

  return payload;
}

function mapNeedle(value, max) {
  const clampedMax = Math.max(max || 1, 0.001);
  const ratio = Math.min(Math.max(value / clampedMax, 0), 1);
  return -90 + ratio * 180;
}

function updateChannelSelect(channels) {
  const current = channelSelect.value;
  channelSelect.innerHTML = "";
  channels.forEach((ch) => {
    const option = document.createElement("option");
    option.value = String(ch.channelNumber);
    option.textContent = `Channel ${ch.channelNumber}`;
    channelSelect.appendChild(option);
  });

  if (channels.length > 0) {
    channelSelect.value = current || String(channels[0].channelNumber);
  }
}

function renderGauges(snapshot) {
  gauges.innerHTML = "";
  snapshot.channels.forEach((ch) => {
    const frag = gaugeTemplate.content.cloneNode(true);

    frag.querySelector(".channel-title").textContent = `CHANNEL ${ch.channelNumber}`;
    frag.querySelector(".digital-value.volts").textContent = `${ch.currentVolts.toFixed(2)} V`;
    frag.querySelector(".digital-value.amps").textContent = `${ch.currentAmps.toFixed(2)} A`;
    frag.querySelector(".mode").textContent = `Mode: ${ch.controlMode}`;
    frag.querySelector(".setpoints").textContent = `Set: ${ch.desiredVolts.toFixed(2)} V / ${ch.desiredAmps.toFixed(2)} A`;

    const voltsDeg = mapNeedle(ch.currentVolts, snapshot.maxVolts);
    const ampsDeg = mapNeedle(ch.currentAmps, snapshot.maxAmps);

    frag.querySelector(".needle-volts").style.transform = `rotate(${voltsDeg}deg)`;
    frag.querySelector(".needle-amps").style.transform = `rotate(${ampsDeg}deg)`;

    gauges.appendChild(frag);
  });
}

function updateConnection(isLive, text) {
  connDot.classList.toggle("live", isLive);
  connText.textContent = text;
}

async function refreshState() {
  const query = state.deviceId ? `?deviceId=${encodeURIComponent(state.deviceId)}` : "";
  const snapshot = await api(`/api/powersupply/state${query}`);

  state.deviceId = snapshot.deviceId;
  state.channels = snapshot.channels;

  renderGauges(snapshot);
  updateChannelSelect(snapshot.channels);
  updateConnection(true, `Connected: ${snapshot.deviceId}`);
}

async function applyChannel() {
  const channel = Number(channelSelect.value);
  const payload = {
    deviceId: state.deviceId,
    volts: Number(voltsInput.value),
    amps: Number(ampsInput.value),
    mode: modeSelect.value
  };

  await api(`/api/powersupply/channel/${channel}/set`, "POST", payload);
  messageEl.textContent = `Updated channel ${channel}`;
  await refreshState();
}

async function setAll(isOn) {
  const endpoint = isOn ? "/api/powersupply/all/on" : "/api/powersupply/all/off";
  await api(endpoint, "POST", { deviceId: state.deviceId });
  messageEl.textContent = isOn ? "All channels turned ON" : "All channels turned OFF";
  await refreshState();
}

document.getElementById("apply-btn").addEventListener("click", async () => {
  try {
    await applyChannel();
  } catch (error) {
    messageEl.textContent = error.message;
  }
});

document.getElementById("all-on-btn").addEventListener("click", async () => {
  try {
    await setAll(true);
  } catch (error) {
    messageEl.textContent = error.message;
  }
});

document.getElementById("all-off-btn").addEventListener("click", async () => {
  try {
    await setAll(false);
  } catch (error) {
    messageEl.textContent = error.message;
  }
});

channelSelect.addEventListener("change", () => {
  const selected = state.channels.find((ch) => String(ch.channelNumber) === channelSelect.value);
  if (!selected) {
    return;
  }

  voltsInput.value = selected.desiredVolts.toFixed(2);
  ampsInput.value = selected.desiredAmps.toFixed(2);
  modeSelect.value = selected.controlMode.toLowerCase();
});

(async () => {
  try {
    await refreshState();
  } catch (error) {
    updateConnection(false, "Device not ready");
    messageEl.textContent = error.message;
  }

  setInterval(async () => {
    try {
      await refreshState();
    } catch {
      updateConnection(false, "Disconnected");
    }
  }, 800);
})();
