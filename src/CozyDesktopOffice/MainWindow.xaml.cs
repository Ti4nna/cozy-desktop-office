<Window x:Class="CozyDesktopOffice.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Cozy Desktop Office"
        WindowStartupLocation="Manual"
        WindowStyle="None"
        AllowsTransparency="True"
        Background="Transparent"
        ShowInTaskbar="False"
        ResizeMode="NoResize"
        Topmost="True"
        Width="1600" Height="900"
        Loaded="MainWindow_Loaded"
        DragEnter="MainWindow_DragEnter"
        Drop="MainWindow_Drop"
        AllowDrop="True">

    <Grid x:Name="RootGrid">
        <Canvas x:Name="OfficeCanvas">
            <Border x:Name="WallPanel" Width="1600" Height="900" Canvas.Left="0" Canvas.Top="0" Background="#FCEAEF" BorderBrush="#E4C4C8" BorderThickness="0"/>
            <Border x:Name="FloorPanel" Width="1600" Height="340" Canvas.Left="0" Canvas.Top="560" Background="#D7B49A"/>
            <Border x:Name="RoomBackWall" Width="1600" Height="190" Canvas.Left="0" Canvas.Top="0" Background="#F5D7DB" Opacity="0.6"/>

            <Rectangle x:Name="SkyGlow" Width="560" Height="360" Canvas.Left="90" Canvas.Top="80" Fill="#F9D8B8" RadiusX="24" RadiusY="24"/>
            <Border x:Name="WindowFrame" Width="620" Height="360" Canvas.Left="60" Canvas.Top="70" Background="#F7E8D9" BorderBrush="#C9968B" BorderThickness="10" CornerRadius="18"/>
            <Rectangle x:Name="WindowGlass" Width="520" Height="260" Canvas.Left="112" Canvas.Top="120" Fill="#B8D9F0"/>
            <Path x:Name="CurtainLeft" Fill="#E8B2BF" Stretch="Fill" Canvas.Left="60" Canvas.Top="70" Width="200" Height="360" Data="M0,0 L200,0 L160,360 L0,360 Z"/>
            <Path x:Name="CurtainRight" Fill="#E8B2BF" Stretch="Fill" Canvas.Left="490" Canvas.Top="70" Width="200" Height="360" Data="M40,0 L200,0 L200,360 L0,360 Z"/>

            <Rectangle x:Name="DeskTop" Width="770" Height="60" Canvas.Left="412" Canvas.Top="520" Fill="#B67F5B" Stroke="#8C5C3C" StrokeThickness="5" RadiusX="8" RadiusY="8"/>
            <Rectangle x:Name="DeskFront" Width="770" Height="160" Canvas.Left="412" Canvas.Top="580" Fill="#8F6048" Stroke="#6C4333" StrokeThickness="4" RadiusX="8" RadiusY="8"/>
            <Rectangle x:Name="DeskLegLeft" Width="28" Height="200" Canvas.Left="455" Canvas.Top="678" Fill="#7A4D3C"/>
            <Rectangle x:Name="DeskLegRight" Width="28" Height="200" Canvas.Left="1080" Canvas.Top="678" Fill="#7A4D3C"/>
            <Rectangle x:Name="DeskLegMidLeft" Width="24" Height="160" Canvas.Left="620" Canvas.Top="700" Fill="#7A4D3C"/>
            <Rectangle x:Name="DeskLegMidRight" Width="24" Height="160" Canvas.Left="932" Canvas.Top="700" Fill="#7A4D3C"/>

            <Rectangle x:Name="MonitorBase" Width="250" Height="22" Canvas.Left="530" Canvas.Top="418" Fill="#4F4B5D" RadiusX="8" RadiusY="8"/>
            <Rectangle x:Name="MonitorScreen" Width="230" Height="150" Canvas.Left="540" Canvas.Top="280" Fill="#A8E1F7" Stroke="#4D6F88" StrokeThickness="8" RadiusX="12" RadiusY="12"/>
            <Rectangle x:Name="MonitorStand" Width="16" Height="90" Canvas.Left="650" Canvas.Top="430" Fill="#6F6779"/>

            <Rectangle x:Name="Keyboard" Width="170" Height="26" Canvas.Left="830" Canvas.Top="456" Fill="#E3C9C6" RadiusX="8" RadiusY="8"/>
            <Rectangle x:Name="Mouse" Width="34" Height="22" Canvas.Left="1010" Canvas.Top="460" Fill="#D0A29C" RadiusX="8" RadiusY="8"/>
            <Ellipse x:Name="LampBase" Width="22" Height="22" Canvas.Left="1170" Canvas.Top="468" Fill="#E8C4AD"/>
            <Rectangle x:Name="LampStem" Width="9" Height="130" Canvas.Left="1178" Canvas.Top="334" Fill="#B57C62"/>
            <Path x:Name="LampShade" Fill="#F5C4B7" Stroke="#C68A7A" StrokeThickness="4" Data="M1160,330 L1210,330 L1230,390 L1140,390 Z"/>

            <Rectangle x:Name="ChairSeat" Width="180" Height="34" Canvas.Left="265" Canvas.Top="435" Fill="#E7BFC9" Stroke="#C88D9A" StrokeThickness="4" RadiusX="18" RadiusY="18"/>
            <Rectangle x:Name="ChairBack" Width="170" Height="150" Canvas.Left="270" Canvas.Top="270" Fill="#EFC8D3" Stroke="#C88D9A" StrokeThickness="4" RadiusX="18" RadiusY="18"/>
            <Rectangle x:Name="ChairLegLeft" Width="18" Height="185" Canvas.Left="310" Canvas.Top="465" Fill="#B87C88"/>
            <Rectangle x:Name="ChairLegRight" Width="18" Height="185" Canvas.Left="390" Canvas.Top="465" Fill="#B87C88"/>
            <Rectangle x:Name="ChairSupport" Width="120" Height="18" Canvas.Left="300" Canvas.Top="545" Fill="#D8A4B0"/>

            <Border x:Name="BookshelfTarget" Width="400" Height="245" Canvas.Left="1060" Canvas.Top="145" Background="#E7D3C9" BorderBrush="#A88273" BorderThickness="6" CornerRadius="14" AllowDrop="True"/>
            <Rectangle x:Name="BookshelfSide" Width="410" Height="245" Canvas.Left="1055" Canvas.Top="140" Fill="Transparent" Stroke="#8F7267" StrokeThickness="4" RadiusX="16" RadiusY="16"/>
            <Rectangle x:Name="Shelf1" Width="360" Height="10" Canvas.Left="1085" Canvas.Top="250" Fill="#A68469"/>
            <Rectangle x:Name="Shelf2" Width="360" Height="10" Canvas.Left="1085" Canvas.Top="330" Fill="#A68469"/>
            <Rectangle x:Name="Shelf3" Width="360" Height="10" Canvas.Left="1085" Canvas.Top="410" Fill="#A68469"/>

            <Border x:Name="BoardTarget" Width="430" Height="260" Canvas.Left="120" Canvas.Top="510" Background="#D9B189" BorderBrush="#A17E5E" BorderThickness="6" CornerRadius="14" AllowDrop="True"/>
            <Rectangle x:Name="BoardFrame" Width="430" Height="260" Canvas.Left="115" Canvas.Top="505" Fill="Transparent" Stroke="#8F6946" StrokeThickness="4" RadiusX="16" RadiusY="16"/>

            <Border x:Name="DrawerUnit" Width="270" Height="130" Canvas.Left="710" Canvas.Top="130" Background="#EBC4CC" BorderBrush="#C28D98" BorderThickness="6" CornerRadius="12"/>
            <Rectangle x:Name="Drawer1" Width="48" Height="90" Canvas.Left="740" Canvas.Top="160" Fill="#D3ABB5" Stroke="#B07D8D" StrokeThickness="2" RadiusX="10" RadiusY="10"/>
            <Rectangle x:Name="Drawer2" Width="48" Height="90" Canvas.Left="795" Canvas.Top="160" Fill="#D3ABB5" Stroke="#B07D8D" StrokeThickness="2" RadiusX="10" RadiusY="10"/>
            <Rectangle x:Name="Drawer3" Width="48" Height="90" Canvas.Left="850" Canvas.Top="160" Fill="#D3ABB5" Stroke="#B07D8D" StrokeThickness="2" RadiusX="10" RadiusY="10"/>
            <Rectangle x:Name="Drawer4" Width="48" Height="90" Canvas.Left="905" Canvas.Top="160" Fill="#D3ABB5" Stroke="#B07D8D" StrokeThickness="2" RadiusX="10" RadiusY="10"/>

            <Rectangle x:Name="Rug" Width="620" Height="120" Canvas.Left="470" Canvas.Top="705" Fill="#D4A7B5" Opacity="0.7" RadiusX="32" RadiusY="32"/>
            <Ellipse x:Name="PlantPot1" Width="58" Height="44" Canvas.Left="210" Canvas.Top="660" Fill="#B47A69"/>
            <Ellipse x:Name="PlantLeaf1" Width="42" Height="120" Canvas.Left="205" Canvas.Top="550" Fill="#7CA376"/>
            <Ellipse x:Name="PlantLeaf2" Width="42" Height="120" Canvas.Left="225" Canvas.Top="552" Fill="#8DB986"/>
            <Ellipse x:Name="PlantLeaf3" Width="42" Height="120" Canvas.Left="245" Canvas.Top="560" Fill="#6F9A6E"/>

            <Ellipse x:Name="PlantPot2" Width="58" Height="44" Canvas.Left="1290" Canvas.Top="660" Fill="#B47A69"/>
            <Ellipse x:Name="PlantLeaf4" Width="42" Height="120" Canvas.Left="1284" Canvas.Top="550" Fill="#7CA376"/>
            <Ellipse x:Name="PlantLeaf5" Width="42" Height="120" Canvas.Left="1301" Canvas.Top="552" Fill="#8DB986"/>
            <Ellipse x:Name="PlantLeaf6" Width="42" Height="120" Canvas.Left="1325" Canvas.Top="560" Fill="#6F9A6E"/>

            <Ellipse x:Name="MusicStation" Width="220" Height="110" Canvas.Left="1180" Canvas.Top="430" Fill="#F2CADA" Stroke="#C67D97" StrokeThickness="4"/>
            <TextBlock x:Name="MusicText" Canvas.Left="1210" Canvas.Top="468" Text="Apple Music" FontSize="20" FontWeight="Bold" Foreground="#5D384D"/>

            <Canvas x:Name="ShelfObjectsCanvas" Canvas.Left="1080" Canvas.Top="145" Width="360" Height="250"/>
            <Canvas x:Name="DeskObjectsCanvas" Canvas.Left="412" Canvas.Top="580" Width="770" Height="180"/>
            <Canvas x:Name="BoardObjectsCanvas" Canvas.Left="120" Canvas.Top="510" Width="430" Height="260"/>
        </Canvas>

        <StackPanel x:Name="TopToolbar" HorizontalAlignment="Right" VerticalAlignment="Top" Margin="0,20,22,0" Orientation="Vertical" Width="300">
            <Border Background="#FFEFF3" CornerRadius="18" BorderBrush="#D9A5AF" BorderThickness="2" Padding="12">
                <StackPanel>
                    <TextBlock Text="Cozy Office Controls" FontSize="19" FontWeight="Bold" Foreground="#4D2D36" Margin="0,0,0,8"/>
                    <ComboBox x:Name="LightingModeCombo" Width="240" SelectedIndex="0" Margin="0,0,0,8">
                        <ComboBoxItem Content="Automatic"/>
                        <ComboBoxItem Content="Day"/>
                        <ComboBoxItem Content="Sunset"/>
                        <ComboBoxItem Content="Night"/>
                    </ComboBox>
                    <Button x:Name="ToggleDesktopButton" Content="Enable Desktop Mode" Margin="0,0,0,8" Click="ToggleDesktopButton_Click"/>
                    <Button x:Name="OpenFoldersButton" Content="Open Cozy Folders" Margin="0,0,0,8" Click="OpenFoldersButton_Click"/>
                    <Button x:Name="AddPhotoButton" Content="Add Bulletin Photo" Margin="0,0,0,8" Click="AddPhotoButton_Click"/>
                    <Button x:Name="ExitButton" Content="Exit" Background="#C77E7E" Foreground="White" Click="ExitButton_Click"/>
                </StackPanel>
            </Border>
        </StackPanel>

        <TextBlock x:Name="StatusText" HorizontalAlignment="Left" VerticalAlignment="Bottom" Margin="20,0,0,20" FontSize="16" Foreground="#3D2730" Background="#FDEFF4" Padding="10,6,10,6" Text="Ready"/>
    </Grid>
</Window>
