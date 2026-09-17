(() => {
    const databaseName = `fieldnotes:${new URL(document.baseURI).pathname}`;

    function openDatabase() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(databaseName, 1);
            request.onupgradeneeded = () => request.result.createObjectStore('study');
            request.onsuccess = () => {
                request.result.onversionchange = () => request.result.close();
                resolve(request.result);
            };
            request.onerror = () => reject(request.error);
            request.onblocked = () => reject(new Error('Close other Fieldnotes tabs and reload to update browser storage.'));
        });
    }

    async function transact(mode, operation) {
        const database = await openDatabase();
        return new Promise((resolve, reject) => {
            const transaction = database.transaction('study', mode);
            let result;
            transaction.oncomplete = () => {
                database.close();
                resolve(result);
            };
            transaction.onabort = transaction.onerror = () => {
                database.close();
                reject(transaction.error ?? new Error('Browser storage could not save this change.'));
            };
            operation(transaction.objectStore('study'), value => { result = value; });
        });
    }

    window.fieldnotesStorage = {
        load: () => transact('readonly', (store, finish) => {
            const request = store.get('current');
            request.onsuccess = () => finish(JSON.stringify(request.result ?? null));
        }),
        trySave: (json, expectedRevision) => {
            const document = JSON.parse(json);
            if (document.schemaVersion !== 1 || document.revision !== expectedRevision + 1) {
                return Promise.reject(new Error('Invalid progress version.'));
            }
            return transact('readwrite', (store, finish) => {
                const request = store.get('current');
                request.onsuccess = () => {
                    if ((request.result?.revision ?? 0) !== expectedRevision) {
                        finish(false);
                        return;
                    }
                    store.put(document, 'current');
                    finish(true);
                };
            });
        },
        download: json => {
            const url = URL.createObjectURL(new Blob([json], { type: 'application/json' }));
            const link = document.createElement('a');
            link.href = url;
            link.download = 'fieldnotes-progress.json';
            document.body.append(link);
            link.click();
            link.remove();
            setTimeout(() => URL.revokeObjectURL(url), 0);
        }
    };
})();