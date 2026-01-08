const { app, BrowserWindow, Menu, ipcMain, dialog, shell } = require('electron');
const path = require('path');
const fs = require('fs');
const { spawn } = require('child_process');

// Keep a global reference of the window object
let mainWindow;
let launcherWindow;
let editorWindow;
let modManagerWindow;
let serverProcess = null;

function createMainWindow() {
  // Create the browser window
  mainWindow = new BrowserWindow({
    width: 1200,
    height: 800,
    minWidth: 800,
    minHeight: 600,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false,
      enableRemoteModule: true
    },
    // icon: path.join(__dirname, '../assets/icon.png'), // Using default icon for now
    titleBarStyle: 'default',
    show: false
  });

  // Load the main launcher page
  mainWindow.loadFile(path.join(__dirname, 'renderer/launcher.html'));

  // Show window when ready
  mainWindow.once('ready-to-show', () => {
    mainWindow.show();
  });

  // Handle window closed
  mainWindow.on('closed', () => {
    mainWindow = null;
  });

  // Open DevTools in development
  if (process.argv.includes('--dev')) {
    mainWindow.webContents.openDevTools();
  }
}

function createEditorWindow() {
  if (editorWindow) {
    editorWindow.focus();
    return;
  }

  editorWindow = new BrowserWindow({
    width: 1400,
    height: 900,
    minWidth: 1000,
    minHeight: 700,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false,
      enableRemoteModule: true
    },
    // icon: path.join(__dirname, '../assets/icon.png'), // Using default icon for now
    parent: mainWindow,
    show: false
  });

  editorWindow.loadFile(path.join(__dirname, 'renderer/editor.html'));

  editorWindow.once('ready-to-show', () => {
    editorWindow.show();
  });

  editorWindow.on('closed', () => {
    editorWindow = null;
  });

  if (process.argv.includes('--dev')) {
    editorWindow.webContents.openDevTools();
  }
}

function createModManagerWindow() {
  if (modManagerWindow) {
    modManagerWindow.focus();
    return;
  }

  modManagerWindow = new BrowserWindow({
    width: 1000,
    height: 800,
    minWidth: 800,
    minHeight: 600,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false,
      enableRemoteModule: true
    },
    // icon: path.join(__dirname, '../assets/icon.png'), // Using default icon for now
    parent: mainWindow,
    show: false
  });

  modManagerWindow.loadFile(path.join(__dirname, 'renderer/modmanager.html'));

  modManagerWindow.once('ready-to-show', () => {
    modManagerWindow.show();
  });

  modManagerWindow.on('closed', () => {
    modManagerWindow = null;
  });

  if (process.argv.includes('--dev')) {
    modManagerWindow.webContents.openDevTools();
  }
}

// App event handlers
app.whenReady().then(() => {
  createMainWindow();
  createMenu();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createMainWindow();
    }
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

// Create application menu
function createMenu() {
  const template = [
    {
      label: 'File',
      submenu: [
        {
          label: 'Open Editor',
          accelerator: 'CmdOrCtrl+E',
          click: () => {
            createEditorWindow();
          }
        },
        { type: 'separator' },
        {
          label: 'Exit',
          accelerator: process.platform === 'darwin' ? 'Cmd+Q' : 'Ctrl+Q',
          click: () => {
            app.quit();
          }
        }
      ]
    },
    {
      label: 'Tools',
      submenu: [
        {
          label: 'LAN Server',
          click: () => {
            startLANServer();
          }
        },
        {
          label: 'LAN Client',
          click: () => {
            startLANClient();
          }
        }
      ]
    },
    {
      label: 'Help',
      submenu: [
        {
          label: 'About',
          click: () => {
            dialog.showMessageBox(mainWindow, {
              type: 'info',
              title: 'About Castle Story Modding Tool',
              message: 'Castle Story Modding Tool v1.6.0',
              detail: 'A comprehensive modding and multiplayer enhancement tool for Castle Story.\n\nBuilt with Electron for cross-platform compatibility.'
            });
          }
        },
        {
          label: 'GitHub Repository',
          click: () => {
            shell.openExternal('https://github.com/CrudePixels/CastleStory-Modding-Tool');
          }
        }
      ]
    }
  ];

  const menu = Menu.buildFromTemplate(template);
  Menu.setApplicationMenu(menu);
}

// IPC handlers for communication with renderer process
ipcMain.handle('get-app-version', () => {
  return app.getVersion();
});

ipcMain.handle('open-editor', () => {
  createEditorWindow();
  return true;
});

ipcMain.handle('start-lan-server', () => {
  startLANServer();
  return true;
});

ipcMain.handle('start-lan-client', () => {
  startLANClient();
  return true;
});

ipcMain.handle('open-external', (event, url) => {
  shell.openExternal(url);
  return true;
});

ipcMain.handle('open-mod-manager', () => {
  createModManagerWindow();
  return true;
});

ipcMain.handle('check-path-exists', (event, filePath) => {
  return { exists: fs.existsSync(filePath) };
});

ipcMain.handle('open-folder-dialog', async (event, title) => {
  const result = await dialog.showOpenDialog(mainWindow, {
    title: title,
    properties: ['openDirectory']
  });
  return result;
});

ipcMain.handle('launch-castle-story-with-mods', async (event, options) => {
  try {
    const steamPath = options.steamPath || 'C:\\Program Files (x86)\\Steam\\steam.exe';
    const gameId = '230190'; // Castle Story Steam ID
    
    if (fs.existsSync(steamPath)) {
      // Launch with mods
      const args = ['-applaunch', gameId];
      
      if (options.enableMods) {
        args.push('-mods');
      }
      
      if (options.enableDebugMode) {
        args.push('-debug');
      }
      
      spawn(steamPath, args, { detached: true });
      return { success: true };
    } else {
      return { success: false, error: 'Steam not found' };
    }
  } catch (error) {
    return { success: false, error: error.message };
  }
});

ipcMain.handle('browse-game-directory', async () => {
  const result = await dialog.showOpenDialog(mainWindow, {
    title: 'Select Castle Story Installation Directory',
    properties: ['openDirectory']
  });
  return result;
});

ipcMain.handle('list-directory', async (event, dirPath) => {
  try {
    if (!fs.existsSync(dirPath)) {
      return { success: false, error: 'Directory does not exist' };
    }
    
    const items = fs.readdirSync(dirPath, { withFileTypes: true });
    const result = items.map(item => ({
      name: item.name,
      isDirectory: item.isDirectory(),
      path: path.join(dirPath, item.name)
    }));
    
    return { success: true, items: result };
  } catch (error) {
    return { success: false, error: error.message };
  }
});

ipcMain.handle('get-file-info', async (event, filePath) => {
  try {
    const stats = fs.statSync(filePath);
    return {
      success: true,
      info: {
        size: stats.size,
        modified: stats.mtime,
        isDirectory: stats.isDirectory()
      }
    };
  } catch (error) {
    return { success: false, error: error.message };
  }
});

ipcMain.handle('open-file-dialog', async (event, title, filters) => {
  const result = await dialog.showOpenDialog(mainWindow, {
    title: title || 'Select File',
    properties: ['openFile'],
    filters: filters || [
      { name: 'All Files', extensions: ['*'] },
      { name: 'DLL Files', extensions: ['dll'] },
      { name: 'Lua Files', extensions: ['lua'] },
      { name: 'JSON Files', extensions: ['json'] }
    ]
  });
  return result;
});

ipcMain.handle('save-file-dialog', async () => {
  const result = await dialog.showSaveDialog(mainWindow, {
    filters: [
      { name: 'Lua Files', extensions: ['lua'] },
      { name: 'JSON Files', extensions: ['json'] },
      { name: 'All Files', extensions: ['*'] }
    ]
  });
  return result;
});

ipcMain.handle('read-file', async (event, filePath) => {
  try {
    const content = fs.readFileSync(filePath, 'utf8');
    return { success: true, content };
  } catch (error) {
    return { success: false, error: error.message };
  }
});

ipcMain.handle('write-file', async (event, filePath, content) => {
  try {
    // Create backup before overwriting
    if (fs.existsSync(filePath)) {
      const backupPath = filePath + '.bak';
      try {
        fs.copyFileSync(filePath, backupPath);
      } catch (backupError) {
        // Silently fail backup - not critical
      }
    }
    
    fs.writeFileSync(filePath, content, 'utf8');
    return { success: true };
  } catch (error) {
    return { success: false, error: error.message };
  }
});

// File watcher for external changes
const fileStats = new Map();

ipcMain.handle('check-file-changed', async (event, filePath) => {
  try {
    if (!fs.existsSync(filePath)) {
      return { changed: false };
    }
    
    const stats = fs.statSync(filePath);
    const lastModified = stats.mtime.getTime();
    const previousStats = fileStats.get(filePath);
    
    if (previousStats && previousStats.mtime !== lastModified) {
      fileStats.set(filePath, { mtime: lastModified, size: stats.size });
      return { changed: true };
    }
    
    fileStats.set(filePath, { mtime: lastModified, size: stats.size });
    return { changed: false };
  } catch (error) {
    return { changed: false, error: error.message };
  }
});

ipcMain.handle('launch-castle-story', async (event, gamePath) => {
  try {
    const steamPath = 'C:\\Program Files (x86)\\Steam\\steam.exe';
    const gameId = '230190'; // Castle Story Steam ID
    
    if (fs.existsSync(steamPath)) {
      spawn(steamPath, ['-applaunch', gameId], { detached: true });
      return { success: true };
    } else {
      return { success: false, error: 'Steam not found' };
    }
  } catch (error) {
    return { success: false, error: error.message };
  }
});

// LAN Server functionality
function startLANServer() {
  if (serverProcess) {
    dialog.showMessageBox(mainWindow, {
      type: 'info',
      title: 'LAN Server',
      message: 'LAN Server is already running!'
    });
    return;
  }

  try {
    // Try to find the LAN Server executable
    const possiblePaths = [
      path.join(__dirname, '../Components/LANServer/bin/Release/LANServer.exe'),
      path.join(__dirname, '../Components/LANServer/bin/Debug/LANServer.exe'),
      path.join(__dirname, '../../Components/LANServer/bin/Release/LANServer.exe'),
      path.join(__dirname, '../../Components/LANServer/bin/Debug/LANServer.exe'),
      path.join(process.cwd(), 'Components/LANServer/bin/Release/LANServer.exe'),
      path.join(process.cwd(), 'Components/LANServer/bin/Debug/LANServer.exe')
    ];

    let serverPath = null;
    for (const testPath of possiblePaths) {
      if (fs.existsSync(testPath)) {
        serverPath = testPath;
        break;
      }
    }

    if (serverPath) {
      const serverDir = path.dirname(serverPath);
      serverProcess = spawn(serverPath, [], {
        cwd: serverDir,
        detached: true,
        stdio: 'ignore'
      });
      serverProcess.unref();
      
      dialog.showMessageBox(mainWindow, {
        type: 'info',
        title: 'LAN Server',
        message: 'LAN Server started successfully!',
        detail: `Server is running from: ${serverPath}`
      });
    } else {
      // Fallback: try to use batch file
      const batchPath = path.join(__dirname, '../Components/LANServer/LaunchLANServer.bat');
      if (fs.existsSync(batchPath)) {
        spawn('cmd.exe', ['/c', batchPath], {
          detached: true,
          stdio: 'ignore'
        });
        dialog.showMessageBox(mainWindow, {
          type: 'info',
          title: 'LAN Server',
          message: 'LAN Server is starting...',
          detail: 'Check the server window for status.'
        });
      } else {
        throw new Error('LAN Server executable not found. Please build the project first.');
      }
    }
  } catch (error) {
    dialog.showMessageBox(mainWindow, {
      type: 'error',
      title: 'LAN Server Error',
      message: 'Failed to start LAN Server',
      detail: error.message
    });
  }
}

function startLANClient() {
  try {
    // Try to find the LAN Client executable
    const possiblePaths = [
      path.join(__dirname, '../Components/LANClient/bin/Release/LANClient.exe'),
      path.join(__dirname, '../Components/LANClient/bin/Debug/LANClient.exe'),
      path.join(__dirname, '../../Components/LANClient/bin/Release/LANClient.exe'),
      path.join(__dirname, '../../Components/LANClient/bin/Debug/LANClient.exe'),
      path.join(process.cwd(), 'Components/LANClient/bin/Release/LANClient.exe'),
      path.join(process.cwd(), 'Components/LANClient/bin/Debug/LANClient.exe')
    ];

    let clientPath = null;
    for (const testPath of possiblePaths) {
      if (fs.existsSync(testPath)) {
        clientPath = testPath;
        break;
      }
    }

    if (clientPath) {
      const clientDir = path.dirname(clientPath);
      spawn(clientPath, [], {
        cwd: clientDir,
        detached: true,
        stdio: 'ignore'
      });
      
      dialog.showMessageBox(mainWindow, {
        type: 'info',
        title: 'LAN Client',
        message: 'LAN Client started successfully!',
        detail: `Client is running from: ${clientPath}`
      });
    } else {
      throw new Error('LAN Client executable not found. Please build the project first.');
    }
  } catch (error) {
    dialog.showMessageBox(mainWindow, {
      type: 'error',
      title: 'LAN Client Error',
      message: 'Failed to start LAN Client',
      detail: error.message
    });
  }
}

// Cleanup on app quit
app.on('before-quit', () => {
  if (serverProcess) {
    serverProcess.kill();
  }
});
