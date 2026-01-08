const { ipcRenderer } = require('electron');

class ModManager {
    constructor() {
        this.gamePath = '';
        this.steamPath = '';
        this.activeMods = [];
        this.allMods = []; // All mods including disabled
        this.currentFilter = 'all';
        this.searchQuery = '';
        this.modConflicts = [];
        
        this.init();
    }

    async init() {
        this.setupEventListeners();
        await this.detectGamePaths();
        this.loadMods();
    }

    setupEventListeners() {
        // Back button
        document.getElementById('back-btn').addEventListener('click', () => {
            this.goBack();
        });

        // Launch Game button
        document.getElementById('launch-game-btn').addEventListener('click', () => {
            this.launchGame();
        });

        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            // Ctrl+L or Cmd+L - Launch game
            if ((e.ctrlKey || e.metaKey) && e.key === 'l') {
                e.preventDefault();
                this.launchGame();
            }
            // Ctrl+R or Cmd+R - Refresh mods
            if ((e.ctrlKey || e.metaKey) && e.key === 'r') {
                e.preventDefault();
                this.refreshMods();
            }
            // Ctrl+A or Cmd+A - Add mod
            if ((e.ctrlKey || e.metaKey) && e.key === 'a') {
                e.preventDefault();
                this.showAddModDialog();
            }
            // Escape - Close modals
            if (e.key === 'Escape') {
                const modal = document.getElementById('add-mod-modal');
                if (modal && modal.classList.contains('show')) {
                    this.hideAddModDialog();
                }
            }
        });

        // Browse buttons
        document.getElementById('browse-game-btn').addEventListener('click', () => {
            this.browseGamePath();
        });

        document.getElementById('browse-steam-btn').addEventListener('click', () => {
            this.browseSteamPath();
        });

        // Add mod button
        document.getElementById('add-mod-btn').addEventListener('click', () => {
            this.showAddModDialog();
        });

        // Close add mod modal
        document.getElementById('close-add-mod-modal').addEventListener('click', () => {
            this.hideAddModDialog();
        });

        // Add mod options
        document.getElementById('browse-mod-file-btn').addEventListener('click', () => {
            this.browseModFile();
        });

        document.getElementById('browse-mod-folder-btn').addEventListener('click', () => {
            this.browseModFolder();
        });

        document.getElementById('create-mod-btn').addEventListener('click', () => {
            this.createNewMod();
        });

        // Search and filter
        document.getElementById('mod-search').addEventListener('input', (e) => {
            this.searchQuery = e.target.value.toLowerCase();
            this.renderMods();
        });

        document.getElementById('refresh-mods-btn').addEventListener('click', () => {
            this.refreshMods();
        });

        // Filter buttons
        document.querySelectorAll('.filter-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
                e.target.classList.add('active');
                this.currentFilter = e.target.dataset.filter;
                this.renderMods();
            });
        });

        // Launch options
        document.getElementById('enable-mods').addEventListener('change', (e) => {
            this.updateModStatus(e.target.checked);
        });

        document.getElementById('enable-memory-patches').addEventListener('change', (e) => {
            this.updateMemoryPatchesStatus(e.target.checked);
        });

        document.getElementById('enable-lan-server').addEventListener('change', (e) => {
            this.updateLanServerStatus(e.target.checked);
        });

        document.getElementById('enable-debug-mode').addEventListener('change', (e) => {
            this.updateDebugModeStatus(e.target.checked);
        });
    }

    async detectGamePaths() {
        // Auto-detect Castle Story installation
        const possibleGamePaths = [
            'C:\\Program Files (x86)\\Steam\\steamapps\\common\\Castle Story',
            'C:\\Program Files\\Steam\\steamapps\\common\\Castle Story',
            'D:\\Steam\\steamapps\\common\\Castle Story',
            'C:\\Program Files (x86)\\Steam\\steamapps\\common\\CastleStory',
            'C:\\Program Files\\Steam\\steamapps\\common\\CastleStory'
        ];

        for (const path of possibleGamePaths) {
            try {
                const result = await ipcRenderer.invoke('check-path-exists', path);
                if (result.exists) {
                    this.gamePath = path;
                    document.getElementById('game-path').value = path;
                    break;
                }
            } catch (error) {
                console.log(`Path ${path} not found`);
            }
        }

        // Auto-detect Steam installation
        const possibleSteamPaths = [
            'C:\\Program Files (x86)\\Steam\\steam.exe',
            'C:\\Program Files\\Steam\\steam.exe',
            'D:\\Steam\\steam.exe'
        ];

        for (const path of possibleSteamPaths) {
            try {
                const result = await ipcRenderer.invoke('check-path-exists', path);
                if (result.exists) {
                    this.steamPath = path;
                    document.getElementById('steam-path').value = path;
                    break;
                }
            } catch (error) {
                console.log(`Steam path ${path} not found`);
            }
        }

        this.updateStatus();
    }

    async browseGamePath() {
        try {
            const result = await ipcRenderer.invoke('open-folder-dialog', 'Select Castle Story Installation Directory');
            
            if (!result.canceled && result.filePaths.length > 0) {
                this.gamePath = result.filePaths[0];
                document.getElementById('game-path').value = this.gamePath;
                this.updateStatus();
            }
        } catch (error) {
            this.showNotification(`Error selecting game path: ${error.message}`, 'error');
        }
    }

    async browseSteamPath() {
        try {
            // Use open-file-dialog for Steam executable
            const result = await ipcRenderer.invoke('open-file-dialog', 'Select Steam Executable', [
                { name: 'Executable Files', extensions: ['exe'] },
                { name: 'All Files', extensions: ['*'] }
            ]);
            
            if (!result.canceled && result.filePaths && result.filePaths.length > 0) {
                this.steamPath = result.filePaths[0];
                document.getElementById('steam-path').value = this.steamPath;
                this.updateStatus();
            }
        } catch (error) {
            this.showNotification(`Error selecting Steam path: ${error.message}`, 'error');
        }
    }

    async loadMods() {
        // Load mods from localStorage or scan mod directory
        const savedMods = JSON.parse(localStorage.getItem('activeMods') || '[]');
        
        if (savedMods.length === 0) {
            // Try to scan for mods in game directory
            await this.scanForMods();
        } else {
            this.activeMods = savedMods;
        }

        this.allMods = [...this.activeMods];
        this.checkModConflicts();
        this.renderMods();
    }

    async scanForMods() {
        if (!this.gamePath) return;

        try {
            const modsPath = this.gamePath + '\\Mods';
            const result = await ipcRenderer.invoke('list-directory', modsPath);
            
            if (result.success) {
                const modFiles = result.items.filter(item => 
                    !item.isDirectory && item.name.endsWith('.dll')
                );

                this.activeMods = modFiles.map(file => ({
                    id: `mod-${file.name}`,
                    name: file.name.replace('.dll', ''),
                    description: 'Mod loaded from Mods directory',
                    version: 'v1.0.0',
                    enabled: true,
                    path: file.path,
                    author: 'Unknown',
                    dependencies: []
                }));

                this.saveMods();
            }
        } catch (error) {
            console.log('Could not scan for mods:', error);
        }
    }

    async refreshMods() {
        this.showLoading('Refreshing mod list...');
        await this.scanForMods();
        this.allMods = [...this.activeMods];
        this.checkModConflicts();
        this.renderMods();
        this.hideLoading();
        this.showNotification('Mod list refreshed', 'success');
    }

    checkModConflicts() {
        this.modConflicts = [];
        const enabledMods = this.activeMods.filter(m => m.enabled);
        
        // Check for duplicate mods
        const modNames = new Map();
        enabledMods.forEach(mod => {
            if (modNames.has(mod.name)) {
                this.modConflicts.push({
                    type: 'duplicate',
                    mods: [modNames.get(mod.name), mod],
                    message: `Duplicate mod: ${mod.name}`
                });
            } else {
                modNames.set(mod.name, mod);
            }
        });

        // Check for dependency conflicts
        enabledMods.forEach(mod => {
            if (mod.dependencies && mod.dependencies.length > 0) {
                mod.dependencies.forEach(dep => {
                    const depMod = enabledMods.find(m => m.name === dep || m.id === dep);
                    if (!depMod) {
                        this.modConflicts.push({
                            type: 'missing_dependency',
                            mod: mod,
                            message: `${mod.name} requires ${dep} but it's not enabled`
                        });
                    }
                });
            }
        });

        this.updateConflictsUI();
    }

    updateConflictsUI() {
        const conflictsSection = document.getElementById('conflicts-section');
        const conflictsList = document.getElementById('conflicts-list');
        const conflictsCount = document.getElementById('conflicts-count');

        if (this.modConflicts.length > 0) {
            conflictsSection.style.display = 'block';
            conflictsCount.textContent = this.modConflicts.length;
            conflictsCount.className = 'status-value error';

            conflictsList.innerHTML = this.modConflicts.map(conflict => `
                <div class="conflict-item">
                    <div class="conflict-icon">⚠️</div>
                    <div class="conflict-info">
                        <h4>${conflict.message}</h4>
                        ${conflict.mods ? `<p>Affected mods: ${conflict.mods.map(m => m.name).join(', ')}</p>` : ''}
                    </div>
                    <button class="btn btn-sm btn-outline" onclick="this.resolveConflict('${conflict.type}')">Resolve</button>
                </div>
            `).join('');
        } else {
            conflictsSection.style.display = 'none';
            conflictsCount.textContent = '0';
            conflictsCount.className = 'status-value';
        }
    }

    renderMods() {
        const modsList = document.getElementById('mods-list');
        
        // Filter and search mods
        let filteredMods = this.activeMods;
        
        // Apply filter
        if (this.currentFilter === 'enabled') {
            filteredMods = filteredMods.filter(m => m.enabled);
        } else if (this.currentFilter === 'disabled') {
            filteredMods = filteredMods.filter(m => !m.enabled);
        } else if (this.currentFilter === 'conflicts') {
            filteredMods = filteredMods.filter(m => 
                this.modConflicts.some(c => 
                    (c.mods && c.mods.some(cm => cm.id === m.id)) || 
                    (c.mod && c.mod.id === m.id)
                )
            );
        }
        
        // Apply search
        if (this.searchQuery) {
            filteredMods = filteredMods.filter(m => 
                m.name.toLowerCase().includes(this.searchQuery) ||
                m.description.toLowerCase().includes(this.searchQuery) ||
                (m.author && m.author.toLowerCase().includes(this.searchQuery))
            );
        }
        
        if (filteredMods.length === 0) {
            modsList.innerHTML = `
                <div class="no-mods">
                    <div class="no-mods-icon">🔍</div>
                    <p>No mods found</p>
                    <p class="no-mods-hint">${this.searchQuery ? 'Try a different search term' : 'Click "Add Mod" to install your first mod'}</p>
                </div>
            `;
            return;
        }

        modsList.innerHTML = filteredMods.map(mod => {
            const hasConflict = this.modConflicts.some(c => 
                (c.mods && c.mods.some(cm => cm.id === mod.id)) || 
                (c.mod && c.mod.id === mod.id)
            );
            
            return `
                <div class="mod-item ${!mod.enabled ? 'disabled' : ''} ${hasConflict ? 'has-conflict' : ''}">
                    <div class="mod-info">
                        <div class="mod-header">
                            <h3>${mod.name}</h3>
                            ${hasConflict ? '<span class="conflict-badge">⚠️ Conflict</span>' : ''}
                        </div>
                        <p>${mod.description || 'No description available'}</p>
                        <div class="mod-meta">
                            <span class="mod-version">${mod.version || 'v1.0.0'}</span>
                            ${mod.author ? `<span class="mod-author">by ${mod.author}</span>` : ''}
                            ${mod.dependencies && mod.dependencies.length > 0 ? 
                                `<span class="mod-deps">Requires: ${mod.dependencies.join(', ')}</span>` : ''}
                        </div>
                        <div class="mod-path">${mod.path || 'Unknown path'}</div>
                    </div>
                    <div class="mod-controls">
                        <label class="toggle-switch" title="${mod.enabled ? 'Disable' : 'Enable'} mod">
                            <input type="checkbox" ${mod.enabled ? 'checked' : ''} data-mod-id="${mod.id}">
                            <span class="slider"></span>
                        </label>
                        <button class="btn btn-sm btn-outline" data-mod-id="${mod.id}" title="View mod details">ℹ️</button>
                        <button class="btn btn-sm btn-danger" data-mod-id="${mod.id}" title="Remove mod">🗑️</button>
                    </div>
                </div>
            `;
        }).join('');

        // Add event listeners to mod controls
        modsList.querySelectorAll('input[type="checkbox"]').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => {
                const modId = e.target.dataset.modId;
                this.toggleMod(modId, e.target.checked);
            });
        });

        modsList.querySelectorAll('.btn-danger').forEach(button => {
            button.addEventListener('click', (e) => {
                const modId = e.target.dataset.modId;
                this.removeMod(modId);
            });
        });

        modsList.querySelectorAll('.btn-outline:not(.btn-danger)').forEach(button => {
            button.addEventListener('click', (e) => {
                const modId = e.target.dataset.modId;
                this.showModDetails(modId);
            });
        });

        this.updateModsCount();
    }

    toggleMod(modId, enabled) {
        const mod = this.activeMods.find(m => m.id === modId);
        if (mod) {
            mod.enabled = enabled;
            this.saveMods();
            this.checkModConflicts();
            this.updateModsCount();
            this.renderMods();
        }
    }

    removeMod(modId) {
        if (confirm('Are you sure you want to remove this mod?')) {
            this.activeMods = this.activeMods.filter(m => m.id !== modId);
            this.renderMods();
            this.saveMods();
        }
    }

    showAddModDialog() {
        document.getElementById('add-mod-modal').classList.add('show');
    }

    hideAddModDialog() {
        document.getElementById('add-mod-modal').classList.remove('show');
    }

    async browseModFile() {
        try {
            const result = await ipcRenderer.invoke('open-file-dialog', 'Select Mod File (.dll)', [
                { name: 'Mod Files', extensions: ['dll'] },
                { name: 'All Files', extensions: ['*'] }
            ]);
            
            if (!result.canceled && result.filePaths && result.filePaths.length > 0) {
                const filePath = result.filePaths[0];
                const fileName = filePath.split('\\').pop();
                
                const newMod = {
                    id: `mod-${Date.now()}`,
                    name: fileName.replace('.dll', ''),
                    description: 'Mod loaded from file',
                    version: 'v1.0.0',
                    enabled: true,
                    path: filePath,
                    author: 'Unknown'
                };
                
                this.activeMods.push(newMod);
                this.allMods = [...this.activeMods];
                this.checkModConflicts();
                this.renderMods();
                this.saveMods();
                this.hideAddModDialog();
                this.showNotification('Mod added successfully', 'success');
            }
        } catch (error) {
            this.showNotification(`Error adding mod: ${error.message}`, 'error');
        }
    }

    async browseModFolder() {
        try {
            const result = await ipcRenderer.invoke('open-folder-dialog', 'Select Mod Folder');
            
            if (!result.canceled && result.filePaths.length > 0) {
                const folderPath = result.filePaths[0];
                const dirResult = await ipcRenderer.invoke('list-directory', folderPath);
                
                if (dirResult.success) {
                    const modFiles = dirResult.items.filter(item => 
                        !item.isDirectory && item.name.endsWith('.dll')
                    );

                    modFiles.forEach(file => {
                        const newMod = {
                            id: `mod-${Date.now()}-${file.name}`,
                            name: file.name.replace('.dll', ''),
                            description: 'Mod loaded from folder',
                            version: 'v1.0.0',
                            enabled: true,
                            path: file.path,
                            author: 'Unknown'
                        };
                        
                        if (!this.activeMods.some(m => m.path === file.path)) {
                            this.activeMods.push(newMod);
                        }
                    });

                    this.allMods = [...this.activeMods];
                    this.checkModConflicts();
                    this.renderMods();
                    this.saveMods();
                    this.hideAddModDialog();
                    this.showNotification(`Added ${modFiles.length} mod(s)`, 'success');
                }
            }
        } catch (error) {
            this.showNotification(`Error adding mods: ${error.message}`, 'error');
        }
    }

    createNewMod() {
        this.hideAddModDialog();
        this.showNotification('Mod creation feature coming soon!', 'info');
        // TODO: Implement mod template creation
    }

    showModDetails(modId) {
        const mod = this.activeMods.find(m => m.id === modId);
        if (!mod) return;

        const details = `
            <h3>${mod.name}</h3>
            <p><strong>Version:</strong> ${mod.version || 'v1.0.0'}</p>
            <p><strong>Author:</strong> ${mod.author || 'Unknown'}</p>
            <p><strong>Path:</strong> ${mod.path || 'Unknown'}</p>
            <p><strong>Description:</strong> ${mod.description || 'No description'}</p>
            ${mod.dependencies && mod.dependencies.length > 0 ? 
                `<p><strong>Dependencies:</strong> ${mod.dependencies.join(', ')}</p>` : ''}
        `;
        
        // Show in a simple alert for now, could be a modal
        alert(details);
    }

    saveMods() {
        localStorage.setItem('activeMods', JSON.stringify(this.activeMods));
    }

    updateModsCount() {
        const enabledMods = this.activeMods.filter(m => m.enabled).length;
        document.getElementById('mods-count').textContent = enabledMods;
    }

    updateModStatus(enabled) {
        // Update all mods based on the global setting
        this.activeMods.forEach(mod => {
            mod.enabled = enabled;
        });
        this.renderMods();
        this.saveMods();
    }

    updateMemoryPatchesStatus(enabled) {
        const status = enabled ? 'Active' : 'Disabled';
        document.getElementById('patches-status').textContent = status;
        document.getElementById('patches-status').className = `status-value ${enabled ? 'active' : 'inactive'}`;
    }

    updateLanServerStatus(enabled) {
        // This would start/stop the LAN server
        console.log('LAN Server:', enabled ? 'Starting' : 'Stopping');
    }

    updateDebugModeStatus(enabled) {
        // This would enable/disable debug mode
        console.log('Debug Mode:', enabled ? 'Enabled' : 'Disabled');
    }

    updateStatus() {
        const gameStatus = this.gamePath ? 'Ready' : 'Not Found';
        const statusElement = document.getElementById('game-status');
        statusElement.textContent = gameStatus;
        statusElement.className = `status-value ${this.gamePath ? 'ready' : 'error'}`;
    }

    async launchGame() {
        if (!this.gamePath && !this.steamPath) {
            this.showNotification('Please set the Castle Story or Steam path first', 'error');
            return;
        }

        this.showLoading('Launching Castle Story with mods...');

        try {
            const launchOptions = {
                gamePath: this.gamePath,
                steamPath: this.steamPath,
                enableMods: document.getElementById('enable-mods').checked,
                enableMemoryPatches: document.getElementById('enable-memory-patches').checked,
                enableLanServer: document.getElementById('enable-lan-server').checked,
                enableDebugMode: document.getElementById('enable-debug-mode').checked,
                activeMods: this.activeMods.filter(m => m.enabled)
            };

            const result = await ipcRenderer.invoke('launch-castle-story-with-mods', launchOptions);
            
            if (result.success) {
                this.showNotification('Castle Story launched successfully!', 'success');
                // Close the mod manager after successful launch
                setTimeout(() => {
                    this.goBack();
                }, 2000);
            } else {
                this.showNotification(`Failed to launch game: ${result.error}`, 'error');
            }
        } catch (error) {
            this.showNotification(`Error launching game: ${error.message}`, 'error');
        } finally {
            this.hideLoading();
        }
    }

    goBack() {
        // Close the mod manager window
        window.close();
    }

    showLoading(message = 'Loading...') {
        const overlay = document.getElementById('loading-overlay');
        const text = overlay.querySelector('p');
        text.textContent = message;
        overlay.classList.add('show');
    }

    hideLoading() {
        const overlay = document.getElementById('loading-overlay');
        overlay.classList.remove('show');
    }

    showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.textContent = message;
        
        // Style the notification
        Object.assign(notification.style, {
            position: 'fixed',
            top: '20px',
            right: '20px',
            padding: '1rem 2rem',
            borderRadius: '4px',
            color: 'white',
            fontWeight: '500',
            zIndex: '1001',
            maxWidth: '400px',
            wordWrap: 'break-word',
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.3)',
            transform: 'translateX(100%)',
            transition: 'transform 0.3s ease'
        });

        // Set background color based on type
        switch (type) {
            case 'success':
                notification.style.background = '#4CAF50';
                break;
            case 'error':
                notification.style.background = '#f44336';
                break;
            case 'warning':
                notification.style.background = '#ff9800';
                break;
            default:
                notification.style.background = '#2196F3';
        }

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Auto remove after 3 seconds
        setTimeout(() => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.parentNode.removeChild(notification);
                }
            }, 300);
        }, 3000);
    }
}

// Initialize the mod manager when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ModManager();
});
