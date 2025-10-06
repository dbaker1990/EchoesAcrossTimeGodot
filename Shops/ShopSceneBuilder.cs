using Godot;
using System;

namespace EchoesAcrossTime.Shops
{
    /// <summary>
    /// Helper script to build the ShopUI scene structure
    /// Run this once to generate the UI, then customize visually
    /// </summary>
    [Tool]
    public partial class ShopSceneBuilder : Node
    {
        [Export] public bool BuildShopUIScene { get; set; } = false;
        
        public override void _Process(double delta)
        {
            if (!Engine.IsEditorHint()) return;
            
            if (BuildShopUIScene)
            {
                BuildShopUIScene = false;
                CreateShopUI();
            }
        }
        
        private void CreateShopUI()
        {
            GD.Print("\n=== BUILDING SHOP UI SCENE ===\n");
            
            // Root control
            var root = new Control();
            root.Name = "ShopUI";
            root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            root.Hide(); // Hidden by default
            
            // Add canvas layer for proper rendering
            var canvasLayer = new CanvasLayer();
            canvasLayer.Name = "CanvasLayer";
            root.AddChild(canvasLayer);
            canvasLayer.Owner = root;
            
            // Semi-transparent background
            var background = new ColorRect();
            background.Name = "Background";
            background.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            background.Color = new Color(0, 0, 0, 0.7f);
            canvasLayer.AddChild(background);
            background.Owner = root;
            
            // Center container
            var centerContainer = new CenterContainer();
            centerContainer.Name = "CenterContainer";
            centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            background.AddChild(centerContainer);
            centerContainer.Owner = root;
            
            // Main panel
            var shopPanel = new PanelContainer();
            shopPanel.Name = "shopPanel";
            shopPanel.CustomMinimumSize = new Vector2(900, 600);
            centerContainer.AddChild(shopPanel);
            shopPanel.Owner = root;
            
            // Margin container
            var margin = new MarginContainer();
            margin.Name = "MarginContainer";
            margin.AddThemeConstantOverride("margin_left", 20);
            margin.AddThemeConstantOverride("margin_right", 20);
            margin.AddThemeConstantOverride("margin_top", 20);
            margin.AddThemeConstantOverride("margin_bottom", 20);
            shopPanel.AddChild(margin);
            margin.Owner = root;
            
            // Main vertical layout
            var mainVBox = new VBoxContainer();
            mainVBox.Name = "MainVBox";
            mainVBox.AddThemeConstantOverride("separation", 10);
            margin.AddChild(mainVBox);
            mainVBox.Owner = root;
            
            // === HEADER ===
            var header = new HBoxContainer();
            header.Name = "Header";
            mainVBox.AddChild(header);
            header.Owner = root;
            
            var shopNameLabel = new Label();
            shopNameLabel.Name = "shopNameLabel";
            shopNameLabel.Text = "Shop Name";
            shopNameLabel.AddThemeFontSizeOverride("font_size", 24);
            header.AddChild(shopNameLabel);
            shopNameLabel.Owner = root;
            
            var spacer1 = new Control();
            spacer1.Name = "Spacer";
            spacer1.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            header.AddChild(spacer1);
            spacer1.Owner = root;
            
            var goldLabel = new Label();
            goldLabel.Name = "goldLabel";
            goldLabel.Text = "Gold: 9999G";
            goldLabel.AddThemeFontSizeOverride("font_size", 20);
            header.AddChild(goldLabel);
            goldLabel.Owner = root;
            
            // Shop description
            var shopDescriptionLabel = new Label();
            shopDescriptionLabel.Name = "shopDescriptionLabel";
            shopDescriptionLabel.Text = "Shop description goes here";
            shopDescriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            mainVBox.AddChild(shopDescriptionLabel);
            shopDescriptionLabel.Owner = root;
            
            // Separator
            var sep1 = new HSeparator();
            mainVBox.AddChild(sep1);
            sep1.Owner = root;
            
            // === TABS ===
            var tabBar = new HBoxContainer();
            tabBar.Name = "TabBar";
            mainVBox.AddChild(tabBar);
            tabBar.Owner = root;
            
            var buyTabButton = new Button();
            buyTabButton.Name = "buyTabButton";
            buyTabButton.Text = "BUY";
            buyTabButton.CustomMinimumSize = new Vector2(100, 40);
            tabBar.AddChild(buyTabButton);
            buyTabButton.Owner = root;
            
            var sellTabButton = new Button();
            sellTabButton.Name = "sellTabButton";
            sellTabButton.Text = "SELL";
            sellTabButton.CustomMinimumSize = new Vector2(100, 40);
            tabBar.AddChild(sellTabButton);
            sellTabButton.Owner = root;
            
            var sep2 = new HSeparator();
            mainVBox.AddChild(sep2);
            sep2.Owner = root;
            
            // === CONTENT ===
            var content = new HSplitContainer();
            content.Name = "Content";
            content.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            content.CustomMinimumSize = new Vector2(0, 400);
            mainVBox.AddChild(content);
            content.Owner = root;
            
            // Left side - Item lists
            var leftPanel = new VBoxContainer();
            leftPanel.Name = "LeftPanel";
            leftPanel.CustomMinimumSize = new Vector2(350, 0);
            content.AddChild(leftPanel);
            leftPanel.Owner = root;
            
            var buyItemList = new ItemList();
            buyItemList.Name = "buyItemList";
            buyItemList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            leftPanel.AddChild(buyItemList);
            buyItemList.Owner = root;
            
            var sellItemList = new ItemList();
            sellItemList.Name = "sellItemList";
            sellItemList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            sellItemList.Visible = false;
            leftPanel.AddChild(sellItemList);
            sellItemList.Owner = root;
            
            // Right side - Details
            var rightPanel = new ScrollContainer();
            rightPanel.Name = "RightPanel";
            rightPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            content.AddChild(rightPanel);
            rightPanel.Owner = root;
            
            var detailsPanel = new VBoxContainer();
            detailsPanel.Name = "detailsPanel";
            detailsPanel.AddThemeConstantOverride("separation", 10);
            rightPanel.AddChild(detailsPanel);
            detailsPanel.Owner = root;
            
            // Item icon
            var itemIcon = new TextureRect();
            itemIcon.Name = "itemIcon";
            itemIcon.CustomMinimumSize = new Vector2(64, 64);
            itemIcon.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
            itemIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            detailsPanel.AddChild(itemIcon);
            itemIcon.Owner = root;
            
            // Item name
            var itemNameLabel = new Label();
            itemNameLabel.Name = "itemNameLabel";
            itemNameLabel.Text = "Item Name";
            itemNameLabel.AddThemeFontSizeOverride("font_size", 20);
            detailsPanel.AddChild(itemNameLabel);
            itemNameLabel.Owner = root;
            
            // Item description
            var itemDescriptionLabel = new Label();
            itemDescriptionLabel.Name = "itemDescriptionLabel";
            itemDescriptionLabel.Text = "Item description...";
            itemDescriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            detailsPanel.AddChild(itemDescriptionLabel);
            itemDescriptionLabel.Owner = root;
            
            var sep3 = new HSeparator();
            detailsPanel.AddChild(sep3);
            sep3.Owner = root;
            
            // Price
            var itemPriceLabel = new Label();
            itemPriceLabel.Name = "itemPriceLabel";
            itemPriceLabel.Text = "Price: 0G";
            detailsPanel.AddChild(itemPriceLabel);
            itemPriceLabel.Owner = root;
            
            // Stock
            var itemStockLabel = new Label();
            itemStockLabel.Name = "itemStockLabel";
            itemStockLabel.Text = "Stock: 0";
            detailsPanel.AddChild(itemStockLabel);
            itemStockLabel.Owner = root;
            
            var sep4 = new HSeparator();
            detailsPanel.AddChild(sep4);
            sep4.Owner = root;
            
            // Quantity control
            var quantityControl = new HBoxContainer();
            quantityControl.Name = "QuantityControl";
            detailsPanel.AddChild(quantityControl);
            quantityControl.Owner = root;
            
            var quantityLabel = new Label();
            quantityLabel.Text = "Quantity:";
            quantityControl.AddChild(quantityLabel);
            quantityLabel.Owner = root;
            
            var quantitySpinBox = new SpinBox();
            quantitySpinBox.Name = "quantitySpinBox";
            quantitySpinBox.MinValue = 1;
            quantitySpinBox.MaxValue = 99;
            quantitySpinBox.Value = 1;
            quantitySpinBox.CustomMinimumSize = new Vector2(100, 0);
            quantityControl.AddChild(quantitySpinBox);
            quantitySpinBox.Owner = root;
            
            // Total cost
            var totalCostLabel = new Label();
            totalCostLabel.Name = "totalCostLabel";
            totalCostLabel.Text = "Total: 0G";
            totalCostLabel.AddThemeFontSizeOverride("font_size", 18);
            detailsPanel.AddChild(totalCostLabel);
            totalCostLabel.Owner = root;
            
            // === FOOTER ===
            var footer = new HBoxContainer();
            footer.Name = "Footer";
            mainVBox.AddChild(footer);
            footer.Owner = root;
            
            var buyButton = new Button();
            buyButton.Name = "buyButton";
            buyButton.Text = "Buy";
            buyButton.CustomMinimumSize = new Vector2(100, 40);
            footer.AddChild(buyButton);
            buyButton.Owner = root;
            
            var sellButton = new Button();
            sellButton.Name = "sellButton";
            sellButton.Text = "Sell";
            sellButton.CustomMinimumSize = new Vector2(100, 40);
            sellButton.Visible = false;
            footer.AddChild(sellButton);
            sellButton.Owner = root;
            
            var spacer2 = new Control();
            spacer2.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            footer.AddChild(spacer2);
            spacer2.Owner = root;
            
            var exitButton = new Button();
            exitButton.Name = "exitButton";
            exitButton.Text = "Exit";
            exitButton.CustomMinimumSize = new Vector2(100, 40);
            footer.AddChild(exitButton);
            exitButton.Owner = root;
            
            // Save as packed scene
            var packedScene = new PackedScene();
            packedScene.Pack(root);
            
            string savePath = "res://Shops/ShopUI.tscn";
            var error = ResourceSaver.Save(packedScene, savePath);
            
            if (error == Error.Ok)
            {
                GD.Print($"✅ ShopUI scene saved to: {savePath}");
                GD.Print("\nNow:");
                GD.Print("1. Open ShopUI.tscn in Godot");
                GD.Print("2. Attach ShopUI.cs script to root node");
                GD.Print("3. Export node references in inspector");
                GD.Print("4. Customize theme and visuals");
            }
            else
            {
                GD.PushError($"Failed to save scene: {error}");
            }
            
            GD.Print("\n=== SHOP UI SCENE BUILT ===\n");
            
            // Print export setup instructions
            PrintExportSetup();
        }
        
        private void PrintExportSetup()
        {
            GD.Print("\n=== EXPORT SETUP FOR SHOPUI.CS ===\n");
            GD.Print("After attaching ShopUI.cs to the root node, set these exports:\n");
            GD.Print("[Export] private Control shopPanel → shopPanel");
            GD.Print("[Export] private Label shopNameLabel → shopNameLabel");
            GD.Print("[Export] private Label shopDescriptionLabel → shopDescriptionLabel");
            GD.Print("[Export] private Label goldLabel → goldLabel");
            GD.Print("[Export] private TextureRect shopkeeperPortrait → (add manually if needed)");
            GD.Print("[Export] private Button buyTabButton → buyTabButton");
            GD.Print("[Export] private Button sellTabButton → sellTabButton");
            GD.Print("[Export] private ItemList buyItemList → buyItemList");
            GD.Print("[Export] private ItemList sellItemList → sellItemList");
            GD.Print("[Export] private Control detailsPanel → detailsPanel");
            GD.Print("[Export] private Label itemNameLabel → itemNameLabel");
            GD.Print("[Export] private Label itemDescriptionLabel → itemDescriptionLabel");
            GD.Print("[Export] private Label itemPriceLabel → itemPriceLabel");
            GD.Print("[Export] private Label itemStockLabel → itemStockLabel");
            GD.Print("[Export] private TextureRect itemIcon → itemIcon");
            GD.Print("[Export] private SpinBox quantitySpinBox → quantitySpinBox");
            GD.Print("[Export] private Label totalCostLabel → totalCostLabel");
            GD.Print("[Export] private Button buyButton → buyButton");
            GD.Print("[Export] private Button sellButton → sellButton");
            GD.Print("[Export] private Button exitButton → exitButton");
            GD.Print("\n=== END EXPORT SETUP ===\n");
        }
    }
}