const requests = new Map();
export function isSupported() {
    return typeof globalThis.navigator?.locks?.request === "function";
}
export function request(id, name, shared, ifAvailable, receiver) {
    if (!isSupported()) throw new Error("Web Locks is unavailable. A supported browser and secure context are required.");
    if (requests.has(id)) throw new Error("Duplicate lock request ID.");
    const controller = new AbortController();
    let unlock;
    const held = new Promise(resolve => { unlock = resolve; });
    const entry = { controller, unlock, closed: false, done: null };
    requests.set(id, entry);
    // The specification prohibits combining ifAvailable with an AbortSignal.
    const options = ifAvailable
        ? { mode: shared ? "shared" : "exclusive", ifAvailable: true }
        : { mode: shared ? "shared" : "exclusive", signal: controller.signal };
    entry.done = Promise.resolve().then(() => navigator.locks.request(name, options, async lock => {
        if (entry.closed) return;
        await receiver.invokeMethodAsync("OnGranted", lock !== null);
        if (lock) await held;
    })).catch(async error => {
        if (!entry.closed) {
            try { await receiver.invokeMethodAsync("OnError", String(error.message ?? error)); }
            catch (callbackError) { console.error("Web Locks callback failed", callbackError); }
        }
    }).finally(() => requests.delete(id));
}
export async function release(id) {
    const entry = requests.get(id);
    if (!entry) return;
    entry.closed = true;
    entry.controller.abort();
    entry.unlock();
    await entry.done;
}