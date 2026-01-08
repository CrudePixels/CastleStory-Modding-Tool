const { ipcRenderer } = require('electron');

class LuaEditor {
    constructor() {
        this.editor = null;
        this.currentFile = null;
        this.isDirty = false;
        this.mode = 'advanced'; // 'easy' or 'advanced'
        this.openTabs = new Map();
        this.activeTabId = null;
        this.gameDirectory = '';
        this.currentDirectory = '';
        this.directoryHistory = [];
        this.fileWatchers = new Map();
        this.autoSaveInterval = null;
        this.settings = this.loadSettings();
        
        this.init();
    }

    async init() {
        try {
            await this.initializeMonaco();
            this.setupEventListeners();
            this.setupMonacoEditor();
            this.setupKeyboardShortcuts();
            this.setupFileWatcher();
            this.setupAutoSave();
        } catch (error) {
            this.showNotification(`Failed to initialize editor: ${error.message}`, 'error');
            console.error('Editor initialization error:', error);
        }
    }

    async initializeMonaco() {
        return new Promise((resolve, reject) => {
            try {
                require.config({ paths: { vs: 'https://unpkg.com/monaco-editor@0.45.0/min/vs' } });
                require(['vs/editor/editor.main'], () => {
                    resolve();
                }, (error) => {
                    reject(new Error(`Failed to load Monaco Editor: ${error.message}`));
                });
            } catch (error) {
                reject(new Error(`Monaco Editor initialization error: ${error.message}`));
            }
        });
    }

    setupEventListeners() {
        // Mode switching
        document.getElementById('easy-mode-btn').addEventListener('click', () => {
            this.switchMode('easy');
        });

        document.getElementById('advanced-mode-btn').addEventListener('click', () => {
            this.switchMode('advanced');
        });

        // File operations
        document.getElementById('open-btn').addEventListener('click', () => {
            this.openFile();
        });

        document.getElementById('new-btn').addEventListener('click', () => {
            this.showNewFileDialog();
        });

        document.getElementById('save-btn').addEventListener('click', () => {
            this.saveFile();
        });

        document.getElementById('save-as-btn').addEventListener('click', () => {
            this.saveAsFile();
        });

        document.getElementById('validate-btn').addEventListener('click', () => {
            this.validateFile();
        });

        // Back button
        document.getElementById('back-btn').addEventListener('click', () => {
            this.goBack();
        });

        // File type selection
        document.getElementById('file-type-select').addEventListener('change', (e) => {
            this.changeFileType(e.target.value);
        });

        // File browser
        document.getElementById('browse-game-btn').addEventListener('click', () => {
            this.browseGameDirectory();
        });

        document.getElementById('set-game-path-btn').addEventListener('click', () => {
            this.browseGameDirectory();
        });

        document.getElementById('refresh-files-btn').addEventListener('click', () => {
            this.refreshFileTree();
        });

        // Load saved game directory
        this.loadGameDirectory();

        // New file dialog
        document.getElementById('close-new-file-modal').addEventListener('click', () => {
            this.hideNewFileDialog();
        });

        document.querySelectorAll('.template-item').forEach(item => {
            item.addEventListener('click', () => {
                const template = item.dataset.template;
                this.createFileFromTemplate(template);
            });
        });

        // Recent files menu
        const recentFilesBtn = document.getElementById('recent-files-btn');
        if (recentFilesBtn) {
            recentFilesBtn.addEventListener('click', () => {
                this.showRecentFilesMenu();
            });
        }

        // Find & Replace buttons
        const findBtn = document.getElementById('find-btn');
        if (findBtn) {
            findBtn.addEventListener('click', () => {
                this.showFindDialog();
            });
        }

        const replaceBtn = document.getElementById('replace-btn');
        if (replaceBtn) {
            replaceBtn.addEventListener('click', () => {
                this.showReplaceDialog();
            });
        }

        // Breadcrumb navigation
        const breadcrumbContainer = document.getElementById('breadcrumb-container');
        if (breadcrumbContainer) {
            breadcrumbContainer.addEventListener('click', (e) => {
                if (e.target.classList.contains('breadcrumb-item')) {
                    const path = e.target.dataset.path;
                    if (path) {
                        this.navigateToDirectory(path);
                    }
                }
            });
        }
    }

    setupMonacoEditor() {
        const editorContainer = document.getElementById('monaco-editor');
        
        // Configure Monaco Editor with Castle Story specific settings
        monaco.languages.setLanguageConfiguration('lua', {
            comments: {
                lineComment: '--',
                blockComment: ['--[[', ']]']
            },
            brackets: [
                ['{', '}'],
                ['[', ']'],
                ['(', ')']
            ],
            autoClosingPairs: [
                { open: '{', close: '}' },
                { open: '[', close: ']' },
                { open: '(', close: ')' },
                { open: '"', close: '"' },
                { open: "'", close: "'" }
            ],
            surroundingPairs: [
                { open: '{', close: '}' },
                { open: '[', close: ']' },
                { open: '(', close: ')' },
                { open: '"', close: '"' },
                { open: "'", close: "'" }
            ]
        });

        // Add Castle Story specific snippets
        monaco.languages.registerCompletionItemProvider('lua', {
            provideCompletionItems: (model, position) => {
                const suggestions = [
                    {
                        label: 'config',
                        kind: monaco.languages.CompletionItemKind.Snippet,
                        insertText: 'local config = {\n    $1\n}\n\nreturn config',
                        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                        documentation: 'Castle Story configuration template'
                    },
                    {
                        label: 'playerCount',
                        kind: monaco.languages.CompletionItemKind.Property,
                        insertText: 'playerCount = $1',
                        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                        documentation: 'Number of players in the game'
                    },
                    {
                        label: 'gameMode',
                        kind: monaco.languages.CompletionItemKind.Property,
                        insertText: 'gameMode = "$1"',
                        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                        documentation: 'Game mode (survival, creative, etc.)'
                    },
                    {
                        label: 'difficulty',
                        kind: monaco.languages.CompletionItemKind.Property,
                        insertText: 'difficulty = "$1"',
                        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                        documentation: 'Game difficulty (easy, normal, hard)'
                    }
                ];
                return { suggestions };
            }
        });
        
        this.editor = monaco.editor.create(editorContainer, {
            value: '-- Welcome to Castle Story Modding Tool Lua Editor\n-- Start typing your Lua code here...\n\n-- Example configuration:\nlocal config = {\n    playerCount = 4,\n    gameMode = "survival",\n    difficulty = "normal",\n    \n    -- Resource settings\n    startingBricks = 100,\n    startingWood = 50,\n    \n    -- Game settings\n    dayLength = 300,\n    nightLength = 180,\n    \n    -- Multiplayer settings\n    maxPlayers = 8,\n    enablePvP = false\n}\n\nreturn config',
            language: 'lua',
            theme: 'vs-dark',
            automaticLayout: true,
            minimap: { enabled: true },
            scrollBeyondLastLine: false,
            wordWrap: 'on',
            lineNumbers: 'on',
            folding: true,
            bracketPairColorization: { enabled: true },
            guides: {
                bracketPairs: true,
                indentation: true
            },
            suggest: {
                showKeywords: true,
                showSnippets: true
            },
            quickSuggestions: {
                other: true,
                comments: false,
                strings: true
            },
            parameterHints: {
                enabled: true
            },
            hover: {
                enabled: true
            },
            formatOnPaste: true,
            formatOnType: true
        });

        // Listen for content changes
        this.editor.onDidChangeModelContent(() => {
            this.isDirty = true;
            this.updateSaveButton();
        });

        // Listen for cursor position changes
        this.editor.onDidChangeCursorPosition((e) => {
            this.updateStatusBar(e.position);
        });

        // Listen for model changes (language switching)
        this.editor.onDidChangeModel((model) => {
            if (model) {
                const language = model.getLanguageId();
                document.getElementById('current-language').textContent = language;
            }
        });
    }

    switchMode(mode) {
        this.mode = mode;
        
        const easyModePanel = document.getElementById('easy-mode-panel');
        const advancedModePanel = document.getElementById('advanced-mode-panel');
        const easyModeBtn = document.getElementById('easy-mode-btn');
        const advancedModeBtn = document.getElementById('advanced-mode-btn');

        if (mode === 'easy') {
            easyModePanel.style.display = 'block';
            advancedModePanel.style.display = 'none';
            easyModeBtn.classList.add('active');
            advancedModeBtn.classList.remove('active');
            this.generateFormFromContent();
        } else {
            easyModePanel.style.display = 'none';
            advancedModePanel.style.display = 'flex';
            easyModeBtn.classList.remove('active');
            advancedModeBtn.classList.add('active');
        }
    }

    async openFile() {
        try {
            const result = await ipcRenderer.invoke('open-file-dialog');
            
            if (!result.canceled && result.filePaths.length > 0) {
                const filePath = result.filePaths[0];
                await this.loadFile(filePath);
            }
        } catch (error) {
            this.showNotification(`Error opening file: ${error.message}`, 'error');
        }
    }

    async loadFile(filePath) {
        this.showLoading('Loading file...');
        
        try {
            const result = await ipcRenderer.invoke('read-file', filePath);
            
            if (result.success) {
                this.currentFile = filePath;
                this.editor.setValue(result.content);
                this.isDirty = false;
                this.updateFileInfo();
                this.updateSaveButton();
                this.detectLanguage(filePath);
                this.addToRecentFiles(filePath);
                this.showNotification('File loaded successfully', 'success');
            } else {
                this.showNotification(`Error loading file: ${result.error}`, 'error');
            }
        } catch (error) {
            this.showNotification(`Error loading file: ${error.message}`, 'error');
        } finally {
            this.hideLoading();
        }
    }

    async saveFile() {
        if (!this.currentFile) {
            await this.saveAsFile();
            return;
        }

        this.showLoading('Saving file...');
        
        try {
            const content = this.editor.getValue();
            const result = await ipcRenderer.invoke('write-file', this.currentFile, content);
            
            if (result.success) {
                this.isDirty = false;
                this.updateSaveButton();
                this.updateFileStatus('Saved');
                this.showNotification('File saved successfully', 'success');
            } else {
                this.showNotification(`Error saving file: ${result.error}`, 'error');
            }
        } catch (error) {
            this.showNotification(`Error saving file: ${error.message}`, 'error');
        } finally {
            this.hideLoading();
        }
    }

    async saveAsFile() {
        try {
            const result = await ipcRenderer.invoke('save-file-dialog');
            
            if (!result.canceled && result.filePath) {
                const content = this.editor.getValue();
                const saveResult = await ipcRenderer.invoke('write-file', result.filePath, content);
                
                if (saveResult.success) {
                    this.currentFile = result.filePath;
                    this.isDirty = false;
                    this.updateFileInfo();
                    this.updateSaveButton();
                    this.detectLanguage(result.filePath);
                    this.showNotification('File saved successfully', 'success');
                } else {
                    this.showNotification(`Error saving file: ${saveResult.error}`, 'error');
                }
            }
        } catch (error) {
            this.showNotification(`Error saving file: ${error.message}`, 'error');
        }
    }

    newFile() {
        if (this.isDirty) {
            if (!confirm('You have unsaved changes. Do you want to continue without saving?')) {
                return;
            }
        }

        this.currentFile = null;
        this.editor.setValue('');
        this.isDirty = false;
        this.updateFileInfo();
        this.updateSaveButton();
        this.editor.setModel(monaco.editor.createModel('', 'lua'));
    }

    detectLanguage(filePath) {
        const extension = filePath.split('.').pop().toLowerCase();
        let language = 'plaintext';

        switch (extension) {
            case 'lua':
                language = 'lua';
                break;
            case 'json':
                language = 'json';
                break;
            case 'xml':
                language = 'xml';
                break;
            case 'csv':
                language = 'csv';
                break;
            case 'js':
                language = 'javascript';
                break;
            case 'css':
                language = 'css';
                break;
            case 'html':
                language = 'html';
                break;
        }

        monaco.editor.setModelLanguage(this.editor.getModel(), language);
        document.getElementById('current-language').textContent = language;
    }

    changeFileType(type) {
        let language = 'plaintext';
        
        switch (type) {
            case 'lua':
                language = 'lua';
                break;
            case 'json':
                language = 'json';
                break;
            case 'xml':
                language = 'xml';
                break;
            case 'csv':
                language = 'csv';
                break;
            case 'txt':
                language = 'plaintext';
                break;
        }

        if (this.editor.getModel()) {
            monaco.editor.setModelLanguage(this.editor.getModel(), language);
            document.getElementById('current-language').textContent = language;
        }
    }

    validateFile() {
        const content = this.editor.getValue();
        const language = this.editor.getModel().getLanguageId();
        
        try {
            switch (language) {
                case 'json':
                    JSON.parse(content);
                    this.showNotification('JSON is valid', 'success');
                    break;
                case 'lua':
                    // Basic Lua syntax validation
                    if (this.validateLuaSyntax(content)) {
                        this.showNotification('Lua syntax appears valid', 'success');
                    } else {
                        this.showNotification('Lua syntax errors detected', 'warning');
                    }
                    break;
                default:
                    this.showNotification('Validation not available for this file type', 'info');
            }
        } catch (error) {
            this.showNotification(`Validation error: ${error.message}`, 'error');
        }
    }

    validateLuaSyntax(content) {
        // Improved Lua syntax validation
        let bracketCount = 0;
        let parenCount = 0;
        let braceCount = 0;
        let inString = false;
        let stringChar = '';
        let inComment = false;
        let errors = [];

        for (let i = 0; i < content.length; i++) {
            const char = content[i];
            const nextChar = i + 1 < content.length ? content[i + 1] : '';
            const prevChar = i > 0 ? content[i - 1] : '';

            // Handle string literals
            if (!inComment && (char === '"' || char === "'") && prevChar !== '\\') {
                if (!inString) {
                    inString = true;
                    stringChar = char;
                } else if (char === stringChar) {
                    inString = false;
                    stringChar = '';
                }
                continue;
            }

            // Skip everything inside strings
            if (inString) continue;

            // Handle comments
            if (char === '-' && nextChar === '-') {
                inComment = true;
                // Skip to end of line
                while (i < content.length && content[i] !== '\n') {
                    i++;
                }
                inComment = false;
                continue;
            }

            if (inComment) continue;

            // Count brackets, parentheses, and braces
            switch (char) {
                case '(':
                    parenCount++;
                    break;
                case ')':
                    parenCount--;
                    if (parenCount < 0) {
                        errors.push(`Unmatched closing parenthesis at position ${i}`);
                    }
                    break;
                case '[':
                    bracketCount++;
                    break;
                case ']':
                    bracketCount--;
                    if (bracketCount < 0) {
                        errors.push(`Unmatched closing bracket at position ${i}`);
                    }
                    break;
                case '{':
                    braceCount++;
                    break;
                case '}':
                    braceCount--;
                    if (braceCount < 0) {
                        errors.push(`Unmatched closing brace at position ${i}`);
                    }
                    break;
            }
        }

        // Check for unclosed strings
        if (inString) {
            errors.push('Unclosed string literal');
        }

        // Check for unmatched opening brackets
        if (parenCount > 0) errors.push(`${parenCount} unmatched opening parenthesis(es)`);
        if (bracketCount > 0) errors.push(`${bracketCount} unmatched opening bracket(s)`);
        if (braceCount > 0) errors.push(`${braceCount} unmatched opening brace(s)`);

        if (errors.length > 0) {
            this.showNotification(`Syntax errors: ${errors.join('; ')}`, 'warning');
            return false;
        }

        return true;
    }

    generateFormFromContent() {
        const content = this.editor.getValue();
        const formContainer = document.getElementById('form-container');
        
        if (!content.trim()) {
            formContainer.innerHTML = '<div class="no-file-message"><p>No content to display in form mode</p></div>';
            return;
        }

        // Determine file type and parse accordingly
        const fileName = this.currentFile ? this.currentFile.split('\\').pop().toLowerCase() : '';
        let formHTML = '';

        if (fileName.includes('config.lua') || content.includes('sv_Settings') || content.includes('bricktronCap')) {
            formHTML = this.parseGamemodeConfig(content);
        } else if (fileName.includes('bricktron') || content.includes('Characters') || content.includes('Bricktron')) {
            formHTML = this.parseBricktronNames(content);
        } else if (fileName.includes('language') || content.includes('Language') || content.includes('Translations')) {
            formHTML = this.parseLanguageFile(content);
        } else if (fileName.includes('faction') || (content.includes('Faction') && (content.includes('Color') || content.includes('colour')))) {
            formHTML = this.parseFactionColors(content);
        } else {
            formHTML = this.parseGenericLuaConfig(content);
        }

        formContainer.innerHTML = formHTML;
        
        // Add event listeners to form elements
        this.setupFormEventListeners();
    }

    parseGamemodeConfig(content) {
        // Parse Castle Story gamemode config.lua files
        const config = this.extractLuaVariables(content);
        
        let html = '';
        
        // Raid Management Section (AI Attack Settings)
        const raidSettings = [
            'playerAttackInterval', 'firstWaveDurationBonus', 'maximumEnemyLevel', 
            'initialEnemyLevel', 'levelClockInterval', 'neutralAttackInterval', 
            'startingCorruptCrystals', 'forcePlayerFirefliesToPlayerCrystal'
        ];
        const hasRaidSettings = raidSettings.some(key => config[key] !== undefined);
        
        if (hasRaidSettings) {
            html += '<div class="form-group"><h4>⚔️ Raid Management (AI Attack Settings)</h4>';
            raidSettings.forEach(key => {
                if (config[key] !== undefined) {
                    html += this.generateFormField(key, config[key], 'Raid Management');
                }
            });
            html += '</div>';
        }
        
        // Corruptron AI Settings Section
        const corruptronSettings = [
            'corruptronCap', 'baseCorruptronOffense', 'baseCorruptronDefense',
            'offenseIncreasePerLevel', 'defenseIncreasePerLevel', 'randomCorruptronCapture'
        ];
        const hasCorruptronSettings = corruptronSettings.some(key => config[key] !== undefined);
        
        if (hasCorruptronSettings) {
            html += '<div class="form-group"><h4>👹 Corruptron AI Settings</h4>';
            corruptronSettings.forEach(key => {
                if (config[key] !== undefined) {
                    html += this.generateFormField(key, config[key], 'Corruptron');
                }
            });
            html += '</div>';
        }
        
        // Resource Settings
        const resourceSettings = [
            'bricktronCap', 'startingWorkersCount', 'startingKnightCount', 
            'startingArcherCount', 'startingBricks', 'startingWood', 
            'resourceMultiplier', 'fireflyCostMultiplier'
        ];
        const hasResourceSettings = resourceSettings.some(key => config[key] !== undefined);
        
        if (hasResourceSettings) {
            html += '<div class="form-group"><h4>🏗️ Resource Settings</h4>';
            resourceSettings.forEach(key => {
                if (config[key] !== undefined) {
                    html += this.generateFormField(key, config[key], 'Resources');
                }
            });
            html += '</div>';
        }
        
        // Global Settings
        const globalSettings = [
            'canDigGround', 'playerRelations', 'gameMode', 'difficulty', 
            'maxPlayers', 'enablePvP'
        ];
        const hasGlobalSettings = globalSettings.some(key => config[key] !== undefined);
        
        if (hasGlobalSettings) {
            html += '<div class="form-group"><h4>🌐 Global Settings</h4>';
            globalSettings.forEach(key => {
                if (config[key] !== undefined) {
                    html += this.generateFormField(key, config[key], 'Global');
                }
            });
            html += '</div>';
        }
        
        // Time of Day Settings
        const timeSettings = [
            'startingTimeOfDay', 'daynightCycleSetting', 'daytimeFactor', 
            'nighttimeFactor', 'pauseTimeOfDay', 'dayLength', 'nightLength'
        ];
        const hasTimeSettings = timeSettings.some(key => config[key] !== undefined);
        
        if (hasTimeSettings) {
            html += '<div class="form-group"><h4>⏰ Time of Day Settings</h4>';
            timeSettings.forEach(key => {
                if (config[key] !== undefined) {
                    html += this.generateFormField(key, config[key], 'Time');
                }
            });
            html += '</div>';
        }
        
        // Legacy/Generic Combat Settings (fallback)
        const combatSettings = ['attackInterval', 'enemyLevel'];
        const hasCombatSettings = combatSettings.some(key => config[key] !== undefined && !hasRaidSettings);
        
        if (hasCombatSettings) {
            html += '<div class="form-group"><h4>⚔️ Combat Settings</h4>';
            combatSettings.forEach(key => {
                if (config[key] !== undefined) {
                    html += this.generateFormField(key, config[key], 'Combat');
                }
            });
            html += '</div>';
        }
        
        return html || '<div class="no-file-message"><p>No gamemode configuration found</p></div>';
    }

    parseBricktronNames(content) {
        // Parse bricktron names files
        const names = this.extractLuaTable(content, 'Characters') || this.extractLuaTable(content, 'Bricktron');
        
        if (!names || names.length === 0) {
            return '<div class="no-file-message"><p>No bricktron names found</p></div>';
        }
        
        let html = '<div class="form-group"><h4>🤖 Bricktron Names</h4>';
        html += `<div class="form-field">
            <label>Total Names: ${names.length}</label>
            <textarea id="bricktron-names" rows="10" data-key="names">${names.join('\n')}</textarea>
        </div>`;
        html += '</div>';
        
        return html;
    }

    parseLanguageFile(content) {
        // Parse language/translation files
        const translations = this.extractLuaTable(content, 'Language') || this.extractLuaTable(content, 'Translations');
        
        if (!translations || Object.keys(translations).length === 0) {
            return '<div class="no-file-message"><p>No translations found</p></div>';
        }
        
        let html = '<div class="form-group"><h4>🌍 Language Translations</h4>';
        
        Object.keys(translations).forEach(key => {
            html += this.generateFormField(key, translations[key], 'Translations');
        });
        
        html += '</div>';
        
        return html;
    }

    parseFactionColors(content) {
        // Parse faction color files
        const colors = this.extractLuaTable(content, 'FactionColors') || this.extractLuaTable(content, 'Faction');
        
        if (!colors || Object.keys(colors).length === 0) {
            return '<div class="no-file-message"><p>No faction colors found</p></div>';
        }
        
        let html = '<div class="form-group"><h4>🎨 Faction Colors</h4>';
        
        Object.keys(colors).forEach(faction => {
            const color = colors[faction];
            html += `
                <div class="form-field">
                    <label for="faction-${faction}">${faction}</label>
                    <div class="color-input-group">
                        <input type="color" id="faction-${faction}" value="${this.parseColorValue(color)}" data-key="${faction}">
                        <input type="text" value="${color}" data-key="${faction}" class="color-text">
                    </div>
                </div>
            `;
        });
        
        html += '</div>';
        
        return html;
    }

    parseGenericLuaConfig(content) {
        // Generic Lua config parser
        const config = this.extractLuaVariables(content);
        
        if (Object.keys(config).length === 0) {
            return '<div class="no-file-message"><p>No configuration variables found</p></div>';
        }
        
        let html = '<div class="form-group"><h4>⚙️ Configuration</h4>';
        
        Object.keys(config).forEach(key => {
            html += this.generateFormField(key, config[key]);
        });
        
        html += '</div>';
        
        return html;
    }

    extractLuaVariables(content) {
        const config = {};
        
        // First, try to extract sv_Settings table if it exists
        // Use a more robust method to handle nested braces
        const svSettingsStart = content.indexOf('sv_Settings');
        if (svSettingsStart !== -1) {
            let braceCount = 0;
            let inTable = false;
            let tableStart = -1;
            let tableEnd = -1;
            
            // Find the opening brace
            for (let i = svSettingsStart; i < content.length; i++) {
                if (content[i] === '{') {
                    if (!inTable) {
                        inTable = true;
                        tableStart = i + 1;
                    }
                    braceCount++;
                } else if (content[i] === '}') {
                    braceCount--;
                    if (braceCount === 0 && inTable) {
                        tableEnd = i;
                        break;
                    }
                }
            }
            
            if (tableStart !== -1 && tableEnd !== -1) {
                const svSettingsContent = content.substring(tableStart, tableEnd);
                const lines = svSettingsContent.split('\n');
                
                for (const line of lines) {
                    const trimmed = line.trim();
                    if (trimmed.includes('=') && !trimmed.startsWith('--')) {
                        const match = trimmed.match(/(\w+)\s*=\s*(.+)/);
                        if (match) {
                            const key = match[1];
                            let value = match[2].replace(/,$/, '').trim();
                            
                            // Remove quotes
                            if ((value.startsWith('"') && value.endsWith('"')) || 
                                (value.startsWith("'") && value.endsWith("'"))) {
                                value = value.slice(1, -1);
                            }
                            
                            // Handle boolean values
                            if (value === 'true' || value === 'false') {
                                config[key] = value === 'true';
                            } else if (!isNaN(value) && value !== '') {
                                config[key] = parseFloat(value);
                            } else {
                                config[key] = value;
                            }
                        }
                    }
                }
            }
        }
        
        // Also extract top-level variables
        const lines = content.split('\n');
        for (const line of lines) {
            const trimmed = line.trim();
            if (trimmed.includes('=') && !trimmed.startsWith('--') && !trimmed.includes('sv_Settings')) {
                const match = trimmed.match(/(\w+)\s*=\s*(.+)/);
                if (match) {
                    const key = match[1];
                    let value = match[2].replace(/,$/, '').trim();
                    
                    // Skip if already in config (from sv_Settings)
                    if (config[key] !== undefined) continue;
                    
                    // Remove quotes
                    if ((value.startsWith('"') && value.endsWith('"')) || 
                        (value.startsWith("'") && value.endsWith("'"))) {
                        value = value.slice(1, -1);
                    }
                    
                    // Handle boolean values
                    if (value === 'true' || value === 'false') {
                        config[key] = value === 'true';
                    } else if (!isNaN(value) && value !== '') {
                        config[key] = parseFloat(value);
                    } else {
                        config[key] = value;
                    }
                }
            }
        }
        
        return config;
    }

    extractLuaTable(content, tableName) {
        // Find the table declaration
        const tableStartPattern = new RegExp(`${tableName}\\s*=\\s*\\{`, 's');
        const startMatch = content.search(tableStartPattern);
        
        if (startMatch === -1) return null;
        
        // Find the matching closing brace, handling nested braces
        let braceCount = 0;
        let inTable = false;
        let tableStart = -1;
        let tableEnd = -1;
        
        for (let i = startMatch; i < content.length; i++) {
            const char = content[i];
            const nextChar = i + 1 < content.length ? content[i + 1] : '';
            
            // Skip string literals
            if (char === '"' || char === "'") {
                const quote = char;
                i++;
                while (i < content.length && (content[i] !== quote || content[i - 1] === '\\')) {
                    i++;
                }
                continue;
            }
            
            // Skip comments
            if (char === '-' && nextChar === '-') {
                while (i < content.length && content[i] !== '\n') {
                    i++;
                }
                continue;
            }
            
            if (char === '{') {
                if (!inTable) {
                    inTable = true;
                    tableStart = i + 1;
                }
                braceCount++;
            } else if (char === '}') {
                braceCount--;
                if (braceCount === 0 && inTable) {
                    tableEnd = i;
                    break;
                }
            }
        }
        
        if (tableStart === -1 || tableEnd === -1) return null;
        
        const tableContent = content.substring(tableStart, tableEnd);
        
        // Parse table entries
        if (tableName === 'Characters' || tableName === 'Bricktron') {
            // Array of strings
            const names = [];
            // Handle both simple arrays and nested structures
            const stringPattern = /["']([^"']+)["']/g;
            let match;
            while ((match = stringPattern.exec(tableContent)) !== null) {
                names.push(match[1]);
            }
            return names.length > 0 ? names : null;
        } else {
            // Key-value pairs
            const pairs = {};
            const lines = tableContent.split('\n');
            for (const line of lines) {
                const trimmed = line.trim();
                // Skip empty lines and comments
                if (!trimmed || trimmed.startsWith('--')) continue;
                
                // Match key = "value" or key = 'value'
                const match = trimmed.match(/(\w+)\s*=\s*["']([^"']+)["']/);
                if (match) {
                    pairs[match[1]] = match[2];
                } else {
                    // Try to match numeric values
                    const numMatch = trimmed.match(/(\w+)\s*=\s*([0-9.]+)/);
                    if (numMatch) {
                        pairs[numMatch[1]] = parseFloat(numMatch[2]);
                    } else {
                        // Try boolean values
                        const boolMatch = trimmed.match(/(\w+)\s*=\s*(true|false)/);
                        if (boolMatch) {
                            pairs[boolMatch[1]] = boolMatch[2] === 'true';
                        }
                    }
                }
            }
            return Object.keys(pairs).length > 0 ? pairs : null;
        }
    }

    parseColorValue(colorStr) {
        // Convert Castle Story color format to hex
        if (colorStr.includes(',')) {
            // RGB format: "255, 0, 0"
            const rgb = colorStr.split(',').map(c => parseInt(c.trim()));
            return `#${rgb.map(c => c.toString(16).padStart(2, '0')).join('')}`;
        }
        return colorStr;
    }

    generateFormField(key, value, category = '') {
        const fieldId = `field-${key}`;
        const label = key.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase());
        
        // Determine input type based on value
        let inputType = 'text';
        let inputValue = value;
        
        if (typeof value === 'boolean' || value === 'true' || value === 'false') {
            inputType = 'checkbox';
            inputValue = value === 'true' || value === true;
        } else if (!isNaN(value) && value !== '') {
            inputType = 'number';
            inputValue = parseFloat(value);
        } else if (key.toLowerCase().includes('color')) {
            inputType = 'color';
            inputValue = this.parseColorValue(value);
        }
        
        // Special handling for certain fields
        if (key === 'gameMode') {
            return `
                <div class="form-field">
                    <label for="${fieldId}">${label}</label>
                    <select id="${fieldId}" data-key="${key}">
                        <option value="survival" ${value === 'survival' ? 'selected' : ''}>Survival</option>
                        <option value="creative" ${value === 'creative' ? 'selected' : ''}>Creative</option>
                        <option value="adventure" ${value === 'adventure' ? 'selected' : ''}>Adventure</option>
                        <option value="custom" ${value === 'custom' ? 'selected' : ''}>Custom</option>
                    </select>
                </div>
            `;
        } else if (key === 'difficulty') {
            return `
                <div class="form-field">
                    <label for="${fieldId}">${label}</label>
                    <select id="${fieldId}" data-key="${key}">
                        <option value="easy" ${value === 'easy' ? 'selected' : ''}>Easy</option>
                        <option value="normal" ${value === 'normal' ? 'selected' : ''}>Normal</option>
                        <option value="hard" ${value === 'hard' ? 'selected' : ''}>Hard</option>
                        <option value="expert" ${value === 'expert' ? 'selected' : ''}>Expert</option>
                    </select>
                </div>
            `;
        } else if (key === 'playerRelations') {
            return `
                <div class="form-field">
                    <label for="${fieldId}">${label} (0=Allied, 1=Neutral, 2=Enemy)</label>
                    <select id="${fieldId}" data-key="${key}">
                        <option value="0" ${value === 0 || value === '0' ? 'selected' : ''}>Allied</option>
                        <option value="1" ${value === 1 || value === '1' ? 'selected' : ''}>Neutral</option>
                        <option value="2" ${value === 2 || value === '2' ? 'selected' : ''}>Enemy</option>
                    </select>
                </div>
            `;
        } else if (key === 'daynightCycleSetting') {
            return `
                <div class="form-field">
                    <label for="${fieldId}">${label} (0=Day/Night, 1=Day Only, 2=Night Only)</label>
                    <select id="${fieldId}" data-key="${key}">
                        <option value="0" ${value === 0 || value === '0' ? 'selected' : ''}>Day/Night Cycle</option>
                        <option value="1" ${value === 1 || value === '1' ? 'selected' : ''}>Day Only</option>
                        <option value="2" ${value === 2 || value === '2' ? 'selected' : ''}>Night Only</option>
                    </select>
                </div>
            `;
        } else if (key === 'enablePvP' || key === 'canDigGround' || key === 'forcePlayerFirefliesToPlayerCrystal' || 
                   key === 'randomCorruptronCapture' || key === 'pauseTimeOfDay') {
            return `
                <div class="form-field">
                    <label for="${fieldId}">${label}</label>
                    <input type="checkbox" id="${fieldId}" ${value === 'true' || value === true ? 'checked' : ''} data-key="${key}">
                </div>
            `;
        }
        
        return `
            <div class="form-field">
                <label for="${fieldId}">${label}</label>
                <input type="${inputType}" id="${fieldId}" value="${inputValue}" data-key="${key}">
            </div>
        `;
    }

    setupFormEventListeners() {
        const inputs = document.querySelectorAll('.form-field input, .form-field select, .form-field textarea');
        inputs.forEach(input => {
            input.addEventListener('change', () => {
                this.updateConfigFromForm();
            });
        });

        // Special handling for color inputs
        const colorInputs = document.querySelectorAll('input[type="color"]');
        colorInputs.forEach(input => {
            input.addEventListener('change', () => {
                const textInput = input.parentElement.querySelector('.color-text');
                if (textInput) {
                    textInput.value = input.value;
                }
                this.updateConfigFromForm();
            });
        });

        // Special handling for color text inputs
        const colorTextInputs = document.querySelectorAll('.color-text');
        colorTextInputs.forEach(input => {
            input.addEventListener('change', () => {
                const colorInput = input.parentElement.querySelector('input[type="color"]');
                if (colorInput) {
                    colorInput.value = this.parseColorValue(input.value);
                }
                this.updateConfigFromForm();
            });
        });
    }

    updateConfigFromForm() {
        const inputs = document.querySelectorAll('.form-field input, .form-field select, .form-field textarea');
        const content = this.editor.getValue();
        let newContent = content;

        inputs.forEach(input => {
            const key = input.dataset.key;
            let value;
            
            // Handle different input types
            if (input.type === 'checkbox') {
                value = input.checked ? 'true' : 'false';
            } else if (input.type === 'number') {
                value = input.value;
            } else {
                value = input.value;
            }
            
            // Escape special regex characters in key
            const escapedKey = key.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
            
            // Try to match within sv_Settings table first
            const svSettingsPattern = new RegExp(`(sv_Settings\\s*=\\s*\\{[^}]*?)(${escapedKey}\\s*=\\s*)[^,\\n]+`, 's');
            if (svSettingsPattern.test(newContent)) {
                newContent = newContent.replace(svSettingsPattern, `$1$2${value}`);
            } else {
                // Fallback to general pattern
                const regex = new RegExp(`(${escapedKey}\\s*=\\s*)[^,\\n]+`, 'g');
                newContent = newContent.replace(regex, `$1${value}`);
            }
        });

        this.editor.setValue(newContent);
        this.isDirty = true;
        this.updateSaveButton();
    }

    updateFileInfo() {
        const fileName = this.currentFile ? this.currentFile.split('\\').pop() : 'Untitled';
        document.getElementById('current-file').textContent = fileName;
        document.getElementById('file-size').textContent = `${this.editor.getValue().length} bytes`;
    }

    updateSaveButton() {
        const saveBtn = document.getElementById('save-btn');
        saveBtn.disabled = !this.isDirty || !this.currentFile;
    }

    updateFileStatus(status) {
        document.getElementById('file-status').textContent = status;
    }

    updateStatusBar(position) {
        document.getElementById('current-line').textContent = position.lineNumber;
        document.getElementById('current-column').textContent = position.column;
    }

    addToRecentFiles(filePath) {
        let recentFiles = JSON.parse(localStorage.getItem('recentFiles') || '[]');
        const fileName = filePath.split('\\').pop();
        
        // Remove if already exists
        recentFiles = recentFiles.filter(file => file.path !== filePath);
        
        // Add to beginning
        recentFiles.unshift({
            name: fileName,
            path: filePath,
            date: new Date().toISOString()
        });
        
        // Keep only last 10 files
        recentFiles = recentFiles.slice(0, 10);
        
        localStorage.setItem('recentFiles', JSON.stringify(recentFiles));
    }

    async browseGameDirectory() {
        try {
            const result = await ipcRenderer.invoke('browse-game-directory');
            
            if (!result.canceled && result.filePaths.length > 0) {
                this.gameDirectory = result.filePaths[0];
                this.currentDirectory = this.gameDirectory;
                document.getElementById('game-path').value = this.gameDirectory;
                
                // Save to localStorage
                localStorage.setItem('gameDirectory', this.gameDirectory);
                
                // Refresh file tree
                await this.refreshFileTree();
                
                this.showNotification('Game directory set successfully', 'success');
            }
        } catch (error) {
            this.showNotification(`Error setting game directory: ${error.message}`, 'error');
        }
    }

    loadGameDirectory() {
        const saved = localStorage.getItem('gameDirectory');
        if (saved) {
            this.gameDirectory = saved;
            this.currentDirectory = saved;
            document.getElementById('game-path').value = saved;
            this.refreshFileTree();
        }
    }

    async refreshFileTree() {
        if (!this.gameDirectory) {
            document.getElementById('file-tree').innerHTML = '<div class="no-files">No game directory set</div>';
            this.updateBreadcrumbs();
            return;
        }

        try {
            const result = await ipcRenderer.invoke('list-directory', this.currentDirectory);
            
            if (result.success) {
                this.renderFileTree(result.items);
                this.updateBreadcrumbs();
            } else {
                document.getElementById('file-tree').innerHTML = `<div class="no-files">Error: ${result.error}</div>`;
                this.updateBreadcrumbs();
            }
        } catch (error) {
            document.getElementById('file-tree').innerHTML = `<div class="no-files">Error loading directory</div>`;
            this.updateBreadcrumbs();
        }
    }

    renderFileTree(items) {
        const fileTree = document.getElementById('file-tree');
        
        if (items.length === 0) {
            fileTree.innerHTML = '<div class="no-files">Directory is empty</div>';
            return;
        }

        // Filter for Castle Story relevant files
        const relevantExtensions = ['.lua', '.json', '.xml', '.csv', '.txt', '.png'];
        const filteredItems = items.filter(item => {
            if (item.isDirectory) return true;
            const ext = item.name.toLowerCase().substring(item.name.lastIndexOf('.'));
            return relevantExtensions.includes(ext);
        });

        if (filteredItems.length === 0) {
            fileTree.innerHTML = '<div class="no-files">No relevant files found</div>';
            return;
        }

        // Sort: directories first, then files
        filteredItems.sort((a, b) => {
            if (a.isDirectory && !b.isDirectory) return -1;
            if (!a.isDirectory && b.isDirectory) return 1;
            return a.name.localeCompare(b.name);
        });

        fileTree.innerHTML = filteredItems.map(item => {
            const icon = item.isDirectory ? '📁' : this.getFileIcon(item.name);
            const className = item.isDirectory ? 'file-item directory' : 'file-item file';
            
            return `
                <div class="${className}" data-path="${item.path}" data-is-directory="${item.isDirectory}">
                    <span class="file-icon">${icon}</span>
                    <span class="file-name">${item.name}</span>
                </div>
            `;
        }).join('');

        // Add click handlers
        fileTree.querySelectorAll('.file-item').forEach(item => {
            item.addEventListener('click', () => {
                const filePath = item.dataset.path;
                const isDirectory = item.dataset.isDirectory === 'true';
                
                if (isDirectory) {
                    this.navigateToDirectory(filePath);
                } else {
                    this.loadFileFromTree(filePath);
                }
            });
        });
    }

    getFileIcon(fileName) {
        const ext = fileName.toLowerCase().substring(fileName.lastIndexOf('.'));
        switch (ext) {
            case '.lua': return '🔧';
            case '.json': return '📋';
            case '.xml': return '📄';
            case '.csv': return '📊';
            case '.txt': return '📝';
            case '.png': return '🖼️';
            default: return '📄';
        }
    }

    async navigateToDirectory(dirPath) {
        if (this.currentDirectory && this.currentDirectory !== dirPath) {
            this.directoryHistory.push(this.currentDirectory);
        }
        this.currentDirectory = dirPath;
        await this.refreshFileTree();
        this.updateBreadcrumbs();
    }

    async loadFileFromTree(filePath) {
        await this.loadFile(filePath);
    }

    showNewFileDialog() {
        document.getElementById('new-file-modal').classList.add('show');
    }

    hideNewFileDialog() {
        document.getElementById('new-file-modal').classList.remove('show');
    }

    createFileFromTemplate(template) {
        this.hideNewFileDialog();
        
        let content = '';
        let fileName = '';
        
        switch (template) {
            case 'config':
                content = this.getGamemodeConfigTemplate();
                fileName = 'config.lua';
                break;
            case 'bricktron':
                content = this.getBricktronNamesTemplate();
                fileName = 'bricktron_names.lua';
                break;
            case 'language':
                content = this.getLanguageFileTemplate();
                fileName = 'language.lua';
                break;
            case 'faction':
                content = this.getFactionColorsTemplate();
                fileName = 'faction_colors.lua';
                break;
            case 'custom':
                content = this.getCustomLuaTemplate();
                fileName = 'custom.lua';
                break;
        }
        
        this.editor.setValue(content);
        this.currentFile = fileName;
        this.isDirty = true;
        this.updateSaveButton();
        this.updateFileInfo();
        
        // Switch to easy mode if it's a template that supports it
        if (template !== 'custom') {
            this.switchMode('easy');
        }
    }

    getGamemodeConfigTemplate() {
        return `-- Castle Story Gamemode Configuration
-- Edit these settings to customize your game mode

sv_Settings = {
	--Raid Management (AI Attack Settings)
	playerAttackInterval = 600,  -- Seconds between player attacks
	firstWaveDurationBonus = 300,  -- Bonus time before first wave
	maximumEnemyLevel = 10,  -- Maximum enemy level
	initialEnemyLevel = 0,  -- Starting enemy level
	levelClockInterval = 480,  -- Seconds between level increases
	neutralAttackInterval = 60,  -- Seconds between neutral attacks
	startingCorruptCrystals = 2,  -- Starting number of corrupt crystals
	forcePlayerFirefliesToPlayerCrystal = true,  -- Force fireflies to player crystal
	
	--Corruptron AI Settings
	corruptronCap = 50,  -- Maximum corruptrons that can spawn
	baseCorruptronOffense = 6,  -- Base attack power
	baseCorruptronDefense = 6,  -- Base defense power
	offenseIncreasePerLevel = 3,  -- Attack increase per level
	defenseIncreasePerLevel = 3,  -- Defense increase per level
	randomCorruptronCapture = false,  -- Allow random corruptron capture
	
	--Resources
	bricktronCap = 100,  -- Maximum bricktrons
	startingWorkersCount = 10,  -- Starting workers
	startingKnightCount = 2,  -- Starting knights
	startingArcherCount = 2,  -- Starting archers
	fireflyCostMultiplier = 0.2,  -- Firefly cost multiplier
	
	--Global Settings
	canDigGround = true,  -- Allow digging
	playerRelations = 2,  -- 0 = allied, 1 = neutral, 2 = enemy
	
	--Time of Day
	startingTimeOfDay = 7,  -- Starting time (0-24)
	daynightCycleSetting = 0,  -- 0 = day/night, 1 = day only, 2 = night only
	daytimeFactor = 1.4,  -- Daytime speed multiplier
	nighttimeFactor = 0.6,  -- Nighttime speed multiplier
	pauseTimeOfDay = false,  -- Pause time progression
	moonlight = nil,  -- Moonlight settings
	ambientColor = nil  -- Ambient color settings
}

Characters = {
	Bricktron = {
		Ref = fy_Bricktron,
		Cost = 1
	},
	Corruptron = {
		Ref = fy_Corruptron,
		Occupation = Occupations.Corruptron,
		Cost = 3
	},
	Biftron = {
		Ref = fy_Biftron,
		Occupation = Occupations.Biftron,
		Cost = 12
	},
	Minitron = {
		Ref = fy_Minitron,
		Occupation = Occupations.Minitron,
		Cost = 1.5
	},
	Magitron = {
		Ref = fy_Magitron,
		Occupation = Occupations.Magitron,
		Cost = 18
	}
}

Registry = {
	currentLevel = sv_Settings.initialEnemyLevel,
	timers = {
		timeForPlayerAttack = nil,
		timeBetweenLevels = nil,
		timeForNeutralAttack = nil
	}
}

InterestPoints = {}`;
    }

    getBricktronNamesTemplate() {
        return `-- Bricktron Names
-- Add or remove names for your bricktron characters

local Characters = {
    "Alice",
    "Bob",
    "Charlie",
    "Diana",
    "Eve",
    "Frank",
    "Grace",
    "Henry",
    "Ivy",
    "Jack",
    "Kate",
    "Liam",
    "Maya",
    "Noah",
    "Olivia",
    "Paul",
    "Quinn",
    "Ruby",
    "Sam",
    "Tina"
}

return Characters`;
    }

    getLanguageFileTemplate() {
        return `-- Language/Translation File
-- Add translations for different languages

local Language = {
    -- Game UI
    ["ui.play"] = "Play",
    ["ui.settings"] = "Settings",
    ["ui.quit"] = "Quit",
    
    -- Game Messages
    ["msg.welcome"] = "Welcome to Castle Story!",
    ["msg.game_over"] = "Game Over",
    ["msg.victory"] = "Victory!",
    
    -- Items
    ["item.brick"] = "Brick",
    ["item.wood"] = "Wood",
    ["item.stone"] = "Stone",
    
    -- Buildings
    ["building.wall"] = "Wall",
    ["building.tower"] = "Tower",
    ["building.gate"] = "Gate"
}

return Language`;
    }

    getFactionColorsTemplate() {
        return `-- Faction Colors
-- Define colors for different factions

local FactionColors = {
    ["Red"] = "255, 0, 0",
    ["Blue"] = "0, 0, 255",
    ["Green"] = "0, 255, 0",
    ["Yellow"] = "255, 255, 0",
    ["Purple"] = "128, 0, 128",
    ["Orange"] = "255, 165, 0",
    ["Pink"] = "255, 192, 203",
    ["Cyan"] = "0, 255, 255"
}

return FactionColors`;
    }

    getCustomLuaTemplate() {
        return `-- Custom Lua Script
-- Write your custom Lua code here

local function main()
    -- Your code goes here
    print("Hello from Castle Story!")
end

-- Call the main function
main()`;
    }

    goBack() {
        // Close the editor window
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

    setupKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Ctrl+S or Cmd+S - Save
            if ((e.ctrlKey || e.metaKey) && e.key === 's') {
                e.preventDefault();
                this.saveFile();
            }
            // Ctrl+O or Cmd+O - Open
            else if ((e.ctrlKey || e.metaKey) && e.key === 'o') {
                e.preventDefault();
                this.openFile();
            }
            // Ctrl+N or Cmd+N - New
            else if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
                e.preventDefault();
                this.showNewFileDialog();
            }
            // Ctrl+F or Cmd+F - Find
            else if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
                e.preventDefault();
                this.showFindDialog();
            }
            // Ctrl+H or Cmd+H - Replace
            else if ((e.ctrlKey || e.metaKey) && e.key === 'h') {
                e.preventDefault();
                this.showReplaceDialog();
            }
            // Ctrl+Shift+P or Cmd+Shift+P - Recent Files
            else if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'P') {
                e.preventDefault();
                this.showRecentFilesMenu();
            }
        });
    }

    setupFileWatcher() {
        // File watcher will be implemented via IPC
        setInterval(async () => {
            if (this.currentFile) {
                try {
                    const result = await ipcRenderer.invoke('check-file-changed', this.currentFile);
                    if (result.changed && !this.isDirty) {
                        const reload = confirm('File has been modified externally. Do you want to reload it?');
                        if (reload) {
                            await this.loadFile(this.currentFile);
                        }
                    }
                } catch (error) {
                    // Silently fail - file watcher is optional
                }
            }
        }, 2000); // Check every 2 seconds
    }

    setupAutoSave() {
        if (this.settings.autoSave) {
            this.autoSaveInterval = setInterval(() => {
                if (this.currentFile && this.isDirty) {
                    this.saveFile().catch(() => {
                        // Silently fail auto-save
                    });
                }
            }, this.settings.autoSaveInterval || 60000); // Default 60 seconds
        }
    }

    loadSettings() {
        const defaultSettings = {
            autoSave: false,
            autoSaveInterval: 60000,
            fontSize: 14,
            wordWrap: true,
            theme: 'vs-dark',
            tabSize: 4
        };
        const saved = localStorage.getItem('editorSettings');
        return saved ? { ...defaultSettings, ...JSON.parse(saved) } : defaultSettings;
    }

    saveSettings() {
        localStorage.setItem('editorSettings', JSON.stringify(this.settings));
    }

    showRecentFilesMenu() {
        const recentFiles = JSON.parse(localStorage.getItem('recentFiles') || '[]');
        if (recentFiles.length === 0) {
            this.showNotification('No recent files', 'info');
            return;
        }

        const menu = document.createElement('div');
        menu.className = 'recent-files-menu';
        menu.style.cssText = `
            position: fixed;
            top: 60px;
            left: 20px;
            background: #2d2d30;
            border: 1px solid #3e3e42;
            border-radius: 4px;
            padding: 0.5rem;
            z-index: 1000;
            max-height: 400px;
            overflow-y: auto;
            min-width: 300px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
        `;

        recentFiles.forEach(file => {
            const item = document.createElement('div');
            item.style.cssText = `
                padding: 0.5rem;
                cursor: pointer;
                border-radius: 2px;
                margin-bottom: 2px;
            `;
            item.innerHTML = `
                <div style="font-weight: 500; color: #fff;">${file.name}</div>
                <div style="font-size: 0.85rem; color: #999; margin-top: 2px;">${file.path}</div>
            `;
            item.addEventListener('mouseenter', () => {
                item.style.background = '#3e3e42';
            });
            item.addEventListener('mouseleave', () => {
                item.style.background = 'transparent';
            });
            item.addEventListener('click', async () => {
                document.body.removeChild(menu);
                await this.loadFile(file.path);
            });
            menu.appendChild(item);
        });

        document.body.appendChild(menu);

        // Close on outside click
        const closeMenu = (e) => {
            if (!menu.contains(e.target)) {
                document.body.removeChild(menu);
                document.removeEventListener('click', closeMenu);
            }
        };
        setTimeout(() => document.addEventListener('click', closeMenu), 100);
    }

    showFindDialog() {
        if (!this.editor) return;
        
        const findWidget = this.editor.getContribution('editor.contrib.findController');
        if (findWidget) {
            findWidget.start({
                forceRevealReplace: false,
                seedSearchStringFromSelection: true
            });
        } else {
            // Fallback: use Monaco's built-in find
            this.editor.getAction('actions.find').run();
        }
    }

    showReplaceDialog() {
        if (!this.editor) return;
        
        const findWidget = this.editor.getContribution('editor.contrib.findController');
        if (findWidget) {
            findWidget.start({
                forceRevealReplace: true,
                seedSearchStringFromSelection: true
            });
        } else {
            // Fallback: use Monaco's built-in replace
            this.editor.getAction('editor.action.startFindReplaceAction').run();
        }
    }

    updateBreadcrumbs() {
        const breadcrumbContainer = document.getElementById('breadcrumb-container');
        if (!breadcrumbContainer) return;

        if (!this.currentDirectory) {
            breadcrumbContainer.innerHTML = '';
            return;
        }

        const parts = this.currentDirectory.split('\\').filter(p => p);
        if (parts.length === 0) {
            breadcrumbContainer.innerHTML = '';
            return;
        }

        let currentPath = '';
        const breadcrumbs = parts.map((part, index) => {
            if (index === 0) {
                currentPath = part + '\\';
            } else {
                currentPath = currentPath + part + (index < parts.length - 1 ? '\\' : '');
            }
            const escapedPath = currentPath.replace(/\\/g, '\\\\');
            return `<span class="breadcrumb-item" data-path="${escapedPath}">${part}</span>`;
        });

        // Add root indicator
        if (this.gameDirectory && this.currentDirectory.startsWith(this.gameDirectory)) {
            breadcrumbContainer.innerHTML = `<span class="breadcrumb-item" data-path="${this.gameDirectory.replace(/\\/g, '\\\\')}">🏠 Root</span> / ${breadcrumbs.join(' / ')}`;
        } else {
            breadcrumbContainer.innerHTML = breadcrumbs.join(' / ');
        }
    }

    async loadFile(filePath, addToTabs = true) {
        // Validate file path
        if (!filePath || typeof filePath !== 'string') {
            this.showNotification('Invalid file path', 'error');
            return;
        }

        // Check file size (limit to 10MB)
        try {
            const fileInfo = await ipcRenderer.invoke('get-file-info', filePath);
            if (fileInfo.success && fileInfo.info.size > 10 * 1024 * 1024) {
                const proceed = confirm('File is larger than 10MB. Loading may be slow. Continue?');
                if (!proceed) return;
            }
        } catch (error) {
            // Continue anyway
        }

        this.showLoading('Loading file...');
        
        try {
            const result = await ipcRenderer.invoke('read-file', filePath);
            
            if (result.success) {
                // Handle tabs if enabled
                if (addToTabs) {
                    // Add to tabs if not already open
                    if (!this.openTabs.has(filePath)) {
                        this.addTab(filePath);
                    }
                    this.switchToTab(filePath);
                } else {
                    // Direct load without tabs
                    this.currentFile = filePath;
                    this.editor.setValue(result.content);
                    this.detectLanguage(filePath);
                }
                
                this.isDirty = false;
                this.updateFileInfo();
                this.updateSaveButton();
                this.addToRecentFiles(filePath);
                this.setupFileWatcherForFile(filePath);
                this.showNotification('File loaded successfully', 'success');
            } else {
                this.showNotification(`Error loading file: ${result.error}`, 'error');
            }
        } catch (error) {
            this.showNotification(`Error loading file: ${error.message}`, 'error');
        } finally {
            this.hideLoading();
        }
    }

    async addTab(filePath) {
        const fileName = filePath.split('\\').pop();
        const tabId = filePath;
        
        // Load file content first
        try {
            const result = await ipcRenderer.invoke('read-file', filePath);
            if (result.success) {
                const language = this.detectLanguageFromPath(filePath);
                const model = monaco.editor.createModel(result.content, language);
                
                this.openTabs.set(tabId, {
                    path: filePath,
                    name: fileName,
                    model: model,
                    isDirty: false
                });

                // Listen for changes in this model
                model.onDidChangeContent(() => {
                    const tab = this.openTabs.get(tabId);
                    if (tab) {
                        tab.isDirty = true;
                        this.renderTabs();
                    }
                });

                this.renderTabs();
            }
        } catch (error) {
            this.showNotification(`Error loading file for tab: ${error.message}`, 'error');
        }
    }

    switchToTab(tabId) {
        if (this.openTabs.has(tabId)) {
            const tab = this.openTabs.get(tabId);
            this.activeTabId = tabId;
            this.currentFile = tab.path;
            this.editor.setModel(tab.model);
            this.updateFileInfo();
            this.renderTabs();
        }
    }

    closeTab(tabId) {
        if (this.openTabs.has(tabId)) {
            const tab = this.openTabs.get(tabId);
            tab.model.dispose();
            this.openTabs.delete(tabId);
            
            if (this.activeTabId === tabId) {
                const remainingTabs = Array.from(this.openTabs.keys());
                if (remainingTabs.length > 0) {
                    this.switchToTab(remainingTabs[0]);
                } else {
                    this.currentFile = null;
                    this.activeTabId = null;
                    this.editor.setModel(monaco.editor.createModel('', 'lua'));
                }
            }
            
            this.renderTabs();
        }
    }

    renderTabs() {
        const tabsContainer = document.getElementById('editor-tabs');
        if (!tabsContainer) return;

        tabsContainer.innerHTML = Array.from(this.openTabs.entries()).map(([tabId, tab]) => {
            const isActive = tabId === this.activeTabId;
            const dirtyIndicator = tab.isDirty ? '●' : '';
            return `
                <div class="editor-tab ${isActive ? 'active' : ''}" data-tab-id="${tabId}">
                    <span class="tab-name">${tab.name}${dirtyIndicator ? ' <span style="color: #ff9800;">' + dirtyIndicator + '</span>' : ''}</span>
                    <span class="tab-close" data-tab-id="${tabId}">×</span>
                </div>
            `;
        }).join('');

        // Add click handlers
        tabsContainer.querySelectorAll('.editor-tab').forEach(tabEl => {
            const tabId = tabEl.dataset.tabId;
            tabEl.querySelector('.tab-name').addEventListener('click', () => {
                this.switchToTab(tabId);
            });
            tabEl.querySelector('.tab-close').addEventListener('click', (e) => {
                e.stopPropagation();
                const tab = this.openTabs.get(tabId);
                if (tab && tab.isDirty) {
                    if (!confirm(`File "${tab.name}" has unsaved changes. Close anyway?`)) {
                        return;
                    }
                }
                this.closeTab(tabId);
            });
        });
    }

    detectLanguageFromPath(filePath) {
        const extension = filePath.split('.').pop().toLowerCase();
        const languageMap = {
            'lua': 'lua',
            'json': 'json',
            'xml': 'xml',
            'csv': 'csv',
            'js': 'javascript',
            'css': 'css',
            'html': 'html'
        };
        return languageMap[extension] || 'plaintext';
    }

    setupFileWatcherForFile(filePath) {
        // Remove old watcher if exists
        if (this.fileWatchers.has(filePath)) {
            clearInterval(this.fileWatchers.get(filePath));
        }

        // Set up new watcher
        const watcher = setInterval(async () => {
            try {
                const result = await ipcRenderer.invoke('check-file-changed', filePath);
                if (result.changed && !this.isDirty) {
                    const reload = confirm('File has been modified externally. Do you want to reload it?');
                    if (reload) {
                        await this.loadFile(filePath);
                    }
                }
            } catch (error) {
                // Silently fail
            }
        }, 2000);

        this.fileWatchers.set(filePath, watcher);
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

// Initialize the editor when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new LuaEditor();
});
