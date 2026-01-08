const { ipcRenderer } = require('electron');

class LauncherApp {
    constructor() {
        this.init();
    }

    async init() {
        await this.loadAppVersion();
        this.setupEventListeners();
        this.checkSystemStatus();
        this.loadRecentFiles();
    }

    async loadAppVersion() {
        try {
            const version = await ipcRenderer.invoke('get-app-version');
            document.getElementById('version').textContent = version;
        } catch (error) {
            console.error('Failed to load app version:', error);
        }
    }

    setupEventListeners() {
        // Editor button
        document.getElementById('editor-btn').addEventListener('click', async () => {
            this.showLoading('Opening Lua Editor...');
            try {
                await ipcRenderer.invoke('open-editor');
                this.hideLoading();
            } catch (error) {
                this.hideLoading();
                this.showNotification('Failed to open editor', 'error');
            }
        });

        // Launch Game button
        document.getElementById('launch-game-btn').addEventListener('click', async () => {
            this.showLoading('Opening Mod Manager...');
            try {
                await ipcRenderer.invoke('open-mod-manager');
                this.hideLoading();
            } catch (error) {
                this.hideLoading();
                this.showNotification('Failed to open Mod Manager', 'error');
            }
        });

        // LAN Server button
        document.getElementById('lan-server-btn').addEventListener('click', async () => {
            this.showLoading('Starting LAN Server...');
            try {
                await ipcRenderer.invoke('start-lan-server');
                this.hideLoading();
                this.updateServerStatus('Running');
            } catch (error) {
                this.hideLoading();
                this.showNotification('Failed to start LAN Server', 'error');
            }
        });

        // LAN Client button
        document.getElementById('lan-client-btn').addEventListener('click', async () => {
            this.showLoading('Starting LAN Client...');
            try {
                await ipcRenderer.invoke('start-lan-client');
                this.hideLoading();
            } catch (error) {
                this.hideLoading();
                this.showNotification('Failed to start LAN Client', 'error');
            }
        });

        // Footer links
        document.getElementById('github-link').addEventListener('click', (e) => {
            e.preventDefault();
            ipcRenderer.invoke('open-external', 'https://github.com/CrudePixels/CastleStory-Modding-Tool');
        });

        document.getElementById('help-link').addEventListener('click', (e) => {
            e.preventDefault();
            this.showHelp();
        });

        document.getElementById('about-link').addEventListener('click', (e) => {
            e.preventDefault();
            this.showAbout();
        });
    }

    async launchGame() {
        this.showLoading('Launching Castle Story...');
        
        try {
            const result = await ipcRenderer.invoke('launch-castle-story');
            
            if (result.success) {
                this.showNotification('Castle Story launched successfully!', 'success');
            } else {
                this.showNotification(`Failed to launch game: ${result.error}`, 'error');
            }
        } catch (error) {
            this.showNotification(`Error launching game: ${error.message}`, 'error');
        } finally {
            this.hideLoading();
        }
    }

    checkSystemStatus() {
        // Check for Steam installation
        this.checkSteamInstallation();
        
        // Check for Castle Story installation
        this.checkCastleStoryInstallation();
        
        // Check LAN Server status
        this.updateServerStatus('Stopped');
    }

    checkSteamInstallation() {
        const steamPaths = [
            'C:\\Program Files (x86)\\Steam\\steam.exe',
            'C:\\Program Files\\Steam\\steam.exe',
            'D:\\Steam\\steam.exe'
        ];

        // For demo purposes, we'll simulate checking
        setTimeout(() => {
            const steamStatus = document.getElementById('steam-status');
            steamStatus.textContent = 'Detected';
            steamStatus.className = 'status-value detected';
        }, 1000);
    }

    checkCastleStoryInstallation() {
        // For demo purposes, we'll simulate checking
        setTimeout(() => {
            const gameStatus = document.getElementById('game-status');
            gameStatus.textContent = 'Detected';
            gameStatus.className = 'status-value detected';
        }, 1500);
    }

    updateServerStatus(status) {
        const serverStatus = document.getElementById('server-status');
        serverStatus.textContent = status;
        serverStatus.className = `status-value ${status.toLowerCase()}`;
    }

    loadRecentFiles() {
        // Load recent files from localStorage
        const recentFiles = JSON.parse(localStorage.getItem('recentFiles') || '[]');
        const fileList = document.getElementById('recent-files-list');
        
        if (recentFiles.length === 0) {
            fileList.innerHTML = '<div class="no-files">No recent files</div>';
            return;
        }

        fileList.innerHTML = recentFiles.map(file => `
            <div class="file-item">
                <div class="file-name">${file.name}</div>
                <div class="file-path">${file.path}</div>
                <div class="file-date">${new Date(file.date).toLocaleDateString()}</div>
            </div>
        `).join('');
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
            borderRadius: '8px',
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
                notification.style.background = 'linear-gradient(135deg, #4CAF50, #45a049)';
                break;
            case 'error':
                notification.style.background = 'linear-gradient(135deg, #f44336, #d32f2f)';
                break;
            case 'warning':
                notification.style.background = 'linear-gradient(135deg, #ff9800, #f57c00)';
                break;
            default:
                notification.style.background = 'linear-gradient(135deg, #2196F3, #1976D2)';
        }

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Auto remove after 5 seconds
        setTimeout(() => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.parentNode.removeChild(notification);
                }
            }, 300);
        }, 5000);
    }

    showHelp() {
        const helpContent = `
            <h3>Castle Story Modding Tool Help</h3>
            <p><strong>Lua Editor:</strong> Edit game configuration files with syntax highlighting and validation.</p>
            <p><strong>Launch Game:</strong> Start Castle Story with your custom mods and settings.</p>
            <p><strong>LAN Server:</strong> Host local multiplayer games without Steam.</p>
            <p><strong>LAN Client:</strong> Connect to local servers with auto-discovery.</p>
            <br>
            <p>For more help, visit our GitHub repository or check the documentation.</p>
        `;
        
        this.showModal('Help', helpContent);
    }

    showAbout() {
        const aboutContent = `
            <h3>Castle Story Modding Tool v1.6.0</h3>
            <p>A comprehensive modding and multiplayer enhancement tool for Castle Story.</p>
            <br>
            <p><strong>Built with:</strong></p>
            <ul>
                <li>Electron - Cross-platform desktop framework</li>
                <li>Node.js - Backend functionality</li>
                <li>Monaco Editor - Code editing capabilities</li>
                <li>Web Technologies - Modern UI/UX</li>
            </ul>
            <br>
            <p><strong>Features:</strong></p>
            <ul>
                <li>Advanced Lua Editor with syntax highlighting</li>
                <li>LAN Multiplayer Server and Client</li>
                <li>Game integration and mod management</li>
                <li>Cross-platform compatibility</li>
            </ul>
        `;
        
        this.showModal('About', aboutContent);
    }

    showModal(title, content) {
        // Create modal overlay
        const overlay = document.createElement('div');
        overlay.className = 'modal-overlay';
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.8);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 1002;
        `;

        // Create modal content
        const modal = document.createElement('div');
        modal.className = 'modal-content';
        modal.style.cssText = `
            background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
            border: 1px solid rgba(255, 255, 255, 0.2);
            border-radius: 15px;
            padding: 2rem;
            max-width: 500px;
            max-height: 80vh;
            overflow-y: auto;
            color: white;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.5);
        `;

        modal.innerHTML = `
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;">
                <h2 style="margin: 0; font-weight: 300;">${title}</h2>
                <button class="modal-close" style="
                    background: none;
                    border: none;
                    color: white;
                    font-size: 1.5rem;
                    cursor: pointer;
                    padding: 0.5rem;
                    border-radius: 50%;
                    transition: background 0.3s ease;
                ">×</button>
            </div>
            <div style="line-height: 1.6;">
                ${content}
            </div>
        `;

        overlay.appendChild(modal);
        document.body.appendChild(overlay);

        // Close modal functionality
        const closeModal = () => {
            document.body.removeChild(overlay);
        };

        modal.querySelector('.modal-close').addEventListener('click', closeModal);
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                closeModal();
            }
        });

        // ESC key to close
        const handleEsc = (e) => {
            if (e.key === 'Escape') {
                closeModal();
                document.removeEventListener('keydown', handleEsc);
            }
        };
        document.addEventListener('keydown', handleEsc);
    }
}

// Initialize the app when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new LauncherApp();
});
