function getSelectedEffectId() {
    const effectDropdown = document.getElementById("installedEffects");
    return effectDropdown.value;
}

function fetchEffectProps() {
    var payload = {};
    payload.property_inspector = 'fetchEffectProps';
    payload.effectId = getSelectedEffectId();
    sendPayloadToPlugin(payload);
}

function handleNewEffectSelection() {
    const selectedEffectId = getSelectedEffectId();
    console.log(`selected effect ID is ${selectedEffectId}`);

    clearAllEffectPropsFromForm();

    setSettings();
    fetchEffectProps();
}