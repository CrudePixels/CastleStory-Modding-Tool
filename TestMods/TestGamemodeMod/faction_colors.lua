-- Faction Colors Configuration
-- This file demonstrates the enhanced faction color system

FactionColors = {
    {
        name = "Blue",
        color = Color(0, 0, 255),
        hex = "#0000FF"
    },
    {
        name = "Green", 
        color = Color(0, 255, 0),
        hex = "#00FF00"
    },
    {
        name = "Orange",
        color = Color(255, 165, 0),
        hex = "#FFA500"
    },
    {
        name = "Purple",
        color = Color(128, 0, 128),
        hex = "#800080"
    },
    {
        name = "Red",
        color = Color(255, 0, 0),
        hex = "#FF0000"
    },
    {
        name = "Yellow",
        color = Color(255, 255, 0),
        hex = "#FFFF00"
    },
    {
        name = "Cyan",
        color = Color(0, 255, 255),
        hex = "#00FFFF"
    },
    {
        name = "Magenta",
        color = Color(255, 0, 255),
        hex = "#FF00FF"
    },
    {
        name = "Gold",
        color = Color(255, 215, 0),
        hex = "#FFD700"
    },
    {
        name = "Silver",
        color = Color(192, 192, 192),
        hex = "#C0C0C0"
    }
}

-- Function to get color by name
function GetFactionColor(name)
    for i, color in ipairs(FactionColors) do
        if color.name == name then
            return color.color
        end
    end
    return Color(255, 255, 255) -- Default white
end

-- Function to get all available colors
function GetAllFactionColors()
    return FactionColors
end

-- Function to add a new color
function AddFactionColor(name, r, g, b)
    local newColor = {
        name = name,
        color = Color(r, g, b),
        hex = string.format("#%02X%02X%02X", r, g, b)
    }
    table.insert(FactionColors, newColor)
end

-- Function to remove a color by name
function RemoveFactionColor(name)
    for i, color in ipairs(FactionColors) do
        if color.name == name then
            table.remove(FactionColors, i)
            break
        end
    end
end
