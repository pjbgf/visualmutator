   M S B A M L   `   `   `  ����H  DVisualMutator, Version=2.0.8.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:VisualMutator.Views.ControlsVisualMutator.Views.Controls  5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   ImageButtonTemplate  �� 
TargetType.    (VisualMutator.Views.Controls.ImageButton �Name ��Name � ToolTip �� RelativeSource �� Triggers P� Property P� Value �� 
TargetName	 �� Value
 �� Property �� 
TargetType �� BasedOn  	TextBrush  ButtonBackgroundBrush  ButtonBorderBrush  ButtonStyleOne  ButtonStyleTwo  ButtonStyleThree�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      7Controls*clr-namespace:VisualMutator.Views.Controls  5      %�  &	        '       7   &	 �    7  &	 �    7  &	 �    7  �� 5      "    62   .R�5   
   � $
 Grid��6   .+�5      �� $ ButtonImage��6   # z 5      ��5      �� *NormalImage+ �� *TemplatedParent+��5      �� *	ImageSize+ �� *TemplatedParent+��5      �� *	ImageSize+ �� *TemplatedParent+5      5       5   
   P� 5       �@J�IsMouseOver . . �5      �� $ ButtonImage��6   	 5      �� *
HoverImage+ �� *TemplatedParent+
 � ��5      P� 5       � �� . . �5      �� $ ButtonImage��6   	 5      �� *PressedImage+ �� *TemplatedParent+
 � ��5      P� 5       � {� .  . �5      �� $ ButtonImage��6   	 5       �� *DisabledImage+ �� *TemplatedParent+
 � ��5!      5#      �� 5%      "   6    67   Z� .�5&   
   �� 
 � ��$	 3q�6$   �� 5'   
   
 � ��$	 5q�6#   �� 5(   
   
 �@  	ImageSize$	 32Q�6&   �� 5)   
   
 � ��$	 3q�6,   �� 5*   
   
 � ��#	 �  6'   �� 5+   
   
 � ��#	 �  6'   �� 5,   
   
 � ��#	 �  6(   �� 5-   
   
 � ��$	 Center=�65   �� 5.   
   
 � ��$	 Center=�67   �� 5/   
   
 � ��8	   6%   50      �� 52      "   6#   8   .�54   
   �� 
 �@  NormalImage$B	 </VisualMutator;component/Resources/button-stop-unpressed.png��6(   �� 55   
   
 �@  DisabledImage$A	 ;/VisualMutator;component/Resources/button-stop-disabled.png��6*   �� 56   
   
 �@�ToolTip$	 
Button One��6$   57      �� 59      "   6#   8   .�5;   
   �� 
 �@  NormalImage$0	 */ImageButton;component/Resources/Happy.png��6(   �� 5<   
   
 �@  
HoverImage$1	 +/ImageButton;component/Resources/Tounge.png��6'   �� 5=   
   
 �@  PressedImage$3	 -/ImageButton;component/Resources/Surprize.png��6)   �� 5>   
   
 �@  DisabledImage$.	 (/ImageButton;component/Resources/Sad.png��6*   �� 5?   
   
 �@�ToolTip$	 
Button Two��6$   5@      �� 5B      "   6%   8   .�5D   
   �� 
 �@  PressedImage$1	 +/ImageButton;component/Resources/Normal.png��6)   �� 5E   
   
 �@�ToolTip$	 Button Three��6$   �� 5F   
   
 �@  BorderVisibility$	 Visible=�6-   5G      5I      