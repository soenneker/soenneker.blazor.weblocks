import test from "node:test";
import assert from "node:assert/strict";
import * as locks from "../src/Soenneker.Blazor.WebLocks/wwwroot/js/weblocksinterop.js";

function start(name, { shared = false, available = false, failCallback = false } = {}) {
    const id = crypto.randomUUID();
    const ready = Promise.withResolvers();
    locks.request(id, name, shared, available, {
        async invokeMethodAsync(method, value) {
            if (method === "OnGranted") {
                if (failCallback) throw new Error("callback disconnected");
                ready.resolve(value);
            } else ready.reject(new Error(value));
        }
    });
    return { id, ready: ready.promise, release: () => locks.release(id) };
}

test("holds an exclusive lock until release, then hands it to the waiter", async () => {
    const name = crypto.randomUUID();
    const first = start(name);
    assert.equal(await first.ready, true);
    const unavailable = start(name, { available: true });
    assert.equal(await unavailable.ready, false);
    const second = start(name);
    assert.ok((await navigator.locks.query()).held.some(x => x.name === name));
    await first.release();
    assert.equal(await second.ready, true);
    await second.release();
    await second.release();
    assert.ok(!(await navigator.locks.query()).held.some(x => x.name === name));
});

test("canceling a queued request removes it and permits the next waiter", async () => {
    const name = crypto.randomUUID();
    const owner = start(name);
    await owner.ready;
    const canceled = start(name);
    await canceled.release();
    const next = start(name);
    await owner.release();
    assert.equal(await next.ready, true);
    await next.release();
    assert.ok(!(await navigator.locks.query()).pending.some(x => x.name === name));
});

test("shared holders coexist but block exclusive acquisition", async () => {
    const name = crypto.randomUUID();
    const a = start(name, { shared: true });
    const b = start(name, { shared: true });
    assert.equal(await a.ready, true);
    assert.equal(await b.ready, true);
    const exclusive = start(name, { available: true });
    assert.equal(await exclusive.ready, false);
    await a.release();
    await b.release();
});

test("cancel immediately, including ifAvailable, without leaking a lock", async () => {
    for (const available of [false, true]) {
        const name = crypto.randomUUID();
        const canceled = start(name, { available });
        await canceled.release();
        const next = start(name, { available: true });
        assert.equal(await next.ready, true);
        await next.release();
    }
});

test("browser validation errors and callback failures release resources", async () => {
    const invalid = start("-reserved");
    await assert.rejects(invalid.ready);
    await invalid.release();
    const name = crypto.randomUUID();
    const failed = start(name, { failCallback: true });
    await assert.rejects(failed.ready, /callback disconnected/);
    await failed.release();
    const next = start(name);
    assert.equal(await next.ready, true);
    await next.release();
});
