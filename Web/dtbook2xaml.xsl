<?xml version="1.0" encoding="utf-8"?>
<!--
  org.daisy.util (C) 2005-2008 Daisy Consortium
  
  This library is free software; you can redistribute it and/or modify it under
  the terms of the GNU Lesser General Public License as published by the Free
  Software Foundation; either version 2.1 of the License, or (at your option)
  any later version.
  
  This library is distributed in the hope that it will be useful, but WITHOUT
  ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
  FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
  details.
  
  You should have received a copy of the GNU Lesser General Public License
  along with this library; if not, write to the Free Software Foundation, Inc.,
  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
-->
<xsl:stylesheet version="2.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:dtb="http://www.daisy.org/z3986/2005/dtbook/"
  xmlns:s="http://www.w3.org/2001/SMIL20/"
  xmlns:m="http://www.w3.org/1998/Math/MathML"
  xmlns:svg="http://www.w3.org/2000/svg"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  xmlns="http://schemas.microsoft.com/client/2007"
  xmlns:bc="clr-namespace:Buttercup.Control;assembly=Buttercup.Control"
  xmlns:controls="clr-namespace:System.Windows.Controls;assembly=System.Windows.Controls.Toolkit"
  exclude-result-prefixes="dtb s m svg xs">

	<xsl:output method="xml" encoding="utf-8" indent="yes" omit-xml-declaration = "yes"/>
	<xsl:strip-space elements="*" />

	<!--
	This is the root template and wraps the output in a StackPanel.
	-->
	<xsl:template match="dtb:book">
		<StackPanel Orientation="Vertical" HorizontalAlignment="Left">
			<xsl:apply-templates/>
		</StackPanel>
	</xsl:template>

	<xsl:template match="dtb:docauthor">
		<Border>
			<TextBlock TextWrapping="Wrap" HorizontalAlignment="Center" >
				<xsl:choose>
					<!-- If this docauthor has a child(ren) then process these within a TextBlock as Runs -->
					<xsl:when test="dtb:sent | dtb:span | dtb:a">
						<xsl:call-template name="copyCommonAttributes" />
						<xsl:apply-templates></xsl:apply-templates>
					</xsl:when>
					<!-- This will catch the default behaviour of docauthor tag - to simply render the inner text. -->
					<xsl:otherwise>
						<xsl:call-template name="copyCommonAttributes" />
						<xsl:attribute name="Text">
							<xsl:value-of select="normalize-space(.)"/>
						</xsl:attribute>
					</xsl:otherwise>
				</xsl:choose>
			</TextBlock>
		</Border>
	</xsl:template>

	<xsl:template match="dtb:p//dtb:pagenum">
		<LineBreak></LineBreak>
		<Run>
			<xsl:call-template name="copyCommonAttributes" />
			<xsl:attribute name="Text">
				-- <xsl:value-of select="."/> --
			</xsl:attribute>
		</Run>
		<LineBreak></LineBreak>
	</xsl:template>

	<xsl:template match="dtb:pagenum">
		<Border>
			<TextBlock HorizontalAlignment="Center">
				<xsl:call-template name="copyCommonAttributes" />
				<xsl:attribute name="Text">
					-- <xsl:value-of select="."/> --
				</xsl:attribute>
			</TextBlock>
		</Border>
	</xsl:template>

	<xsl:template match="dtb:imggroup">
		<xsl:apply-templates />
	</xsl:template>

	<xsl:template match="dtb:img">
		<Border>
			<Image Source="{@src}" Stretch="None" HorizontalAlignment="Center" Margin="10">
				<xsl:call-template name="copyCommonAttributes" />
				<xsl:attribute name="AutomationProperties.HelpText">
					<xsl:value-of select="@alt"/>
				</xsl:attribute>
				<ToolTipService.ToolTip>
					<bc:ExtendedToolTip Content="{@alt}"></bc:ExtendedToolTip>
				</ToolTipService.ToolTip>
			</Image>
		</Border>
	</xsl:template>

	<xsl:template match="dtb:caption">
		<Border>
			<TextBlock HorizontalAlignment="Center" TextWrapping="Wrap">
				<xsl:call-template name="copyCommonAttributes" />
				<xsl:attribute name="Text">
					<xsl:value-of select="."/>
				</xsl:attribute>
			</TextBlock>
		</Border>
	</xsl:template>

	<xsl:template match="dtb:h1">
		<Border>
			<TextBlock TextWrapping="Wrap" FontSize="20" Margin="0,10,0,10">
				<xsl:choose>
					<!-- If this h1 has a child(ren) then process these within a TextBlock as Runs -->
					<xsl:when test="dtb:sent | dtb:span | dtb:a">
						<xsl:call-template name="copyCommonAttributes" />
						<xsl:apply-templates></xsl:apply-templates>
					</xsl:when>
					<!-- This will catch the default behaviour of h1 tag - to simply render the inner text. -->
					<xsl:otherwise>
						<xsl:call-template name="copyCommonAttributes" />
						<xsl:attribute name="Text">
							<xsl:value-of select="normalize-space(.)"/>
						</xsl:attribute>
					</xsl:otherwise>
				</xsl:choose>
			</TextBlock>
		</Border>
	</xsl:template>

	<xsl:template match="dtb:h2">
		<Border>
			<TextBlock TextWrapping="Wrap" FontSize="15" Margin="0,10,0,10">
				<xsl:choose>
					<!-- If this h2 has a child(ren) then process these within a TextBlock as Runs -->
					<xsl:when test="dtb:sent | dtb:span | dtb:a">
						<xsl:call-template name="copyCommonAttributes" />
						<xsl:apply-templates></xsl:apply-templates>
					</xsl:when>
					<!-- This will catch the default behaviour of h2 tag - to simply render the inner text. -->
					<xsl:otherwise>
						<xsl:call-template name="copyCommonAttributes" />
						<xsl:attribute name="Text">
							<xsl:value-of select="normalize-space(.)"/>
						</xsl:attribute>
					</xsl:otherwise>
				</xsl:choose>
			</TextBlock>
		</Border>
	</xsl:template>

	<xsl:template match="dtb:h3">
		<Border>
			<TextBlock TextWrapping="Wrap" FontSize="15" Margin="0,10,0,10" >
				<xsl:choose>
					<!-- If this h3 has a child(ren) then process these within a TextBlock as Runs -->
					<xsl:when test="dtb:sent | dtb:span | dtb:a">
						<xsl:call-template name="copyCommonAttributes" />
						<xsl:apply-templates></xsl:apply-templates>
					</xsl:when>
					<!-- This will catch the default behaviour of h3 tag - to simply render the inner text. -->
					<xsl:otherwise>
						<xsl:call-template name="copyCommonAttributes" />
						<xsl:attribute name="Text">
							<xsl:value-of select="normalize-space(.)"/>
						</xsl:attribute>
					</xsl:otherwise>
				</xsl:choose>
			</TextBlock>
		</Border>
	</xsl:template>

	<xsl:template match="dtb:p">
		<xsl:choose>
			<!-- If this p has a child(ren) then process these within a TextBlock as Runs -->
			<xsl:when test="dtb:sent | dtb:span | dtb:a">
				<Border>
					<TextBlock TextWrapping="Wrap" Margin="0, 0, 0, 10" >
						<xsl:call-template name="copyCommonAttributes" />
						<xsl:apply-templates select="dtb:sent | dtb:span | dtb:a" />
					</TextBlock>
				</Border>
			</xsl:when>
			<!-- imggroups within p's have their own template and shouldn't be wrapped in a TextBlock
			like sentences are -->
			<xsl:when test="dtb:imggroup | dtb:img">
				<controls:WrapPanel Orientation="Horizontal" HorizontalAlignment="Center" >
					<xsl:apply-templates/>
				</controls:WrapPanel>
			</xsl:when>
			<!-- This will catch the default behaviour of p tag - to simply render the inner text. -->
			<xsl:otherwise>
				<Border>
					<TextBlock TextWrapping="Wrap" Margin="0, 0, 0, 10" >
						<xsl:call-template name="copyCommonAttributes" />
						<xsl:value-of select="."/>
					</TextBlock>
				</Border>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="dtb:lic">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="dtb:lic[@smilref]">
		<TextBlock TextWrapping="Wrap" >
			<xsl:call-template name="copyCommonAttributes" />
			<xsl:value-of select="."/>
		</TextBlock>
	</xsl:template>


	<xsl:template match="dtb:em">
		<Run FontStyle ="Italic">
			<xsl:call-template name="copyCommonAttributes" />
			<xsl:call-template name="writeContents" />
		</Run>
	</xsl:template>

	<!-- To ensure sentences in paragraphs are inline we add them as Run elements within the 
	paragraphs's TextBlock. Problem is that they aren't selectable (not events). Living with this
	for now. 
	-->
	<xsl:template match="dtb:sent">
		<xsl:if test="text()">
			<Border>
				<TextBlock TextWrapping="Wrap">
					<xsl:call-template name="copyCommonAttributes" />
					<xsl:value-of select="."/>
				</TextBlock>
			</Border>
		</xsl:if>
	</xsl:template>

	<xsl:template match="dtb:p/dtb:sent | dtb:h1/dtb:sent | dtb:h2/dtb:sent | dtb:h3/dtb:sent | dtb:docauthor/dtb:sent">
		<xsl:choose>
			<!-- 
          If the sent contains text and has a smilref attribute then process it as another Run. 
          Note that some sent's can contain text but no smilref - for example, we have seen cases
          where a fullstop finds its way into a sent.
		-->
			<xsl:when test="text() and @smilref">
				<Run>
					<xsl:call-template name="copyCommonAttributes" />
					<xsl:call-template name="writeContents" />
				</Run>
			</xsl:when>
			<xsl:otherwise>
				<!-- 
            If the sent contains child nodes (like anchors, spans etc) then process those templates
        -->
				<xsl:apply-templates />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="dtb:p//dtb:a">
		<Run>
			<xsl:call-template name="copyCommonAttributes" />
			<xsl:call-template name="writeContents" />
		</Run>
	</xsl:template>

	<!-- All other children of a sent are displayed as runs -->
	<xsl:template match="dtb:p//dtb:span">
		<Run>
			<xsl:call-template name="copyCommonAttributes" />
			<xsl:call-template name="writeContents" />
		</Run>
	</xsl:template>

	<xsl:template match="dtb:p//dtb:strong">
		<Run FontWeight="Bold">
			<xsl:call-template name="copyCommonAttributes" />
			<xsl:call-template name="writeContents" />
		</Run>
	</xsl:template>

	<xsl:template match="dtb:list">
		<xsl:apply-templates />
	</xsl:template>

	<xsl:template match="dtb:list/dtb:li">
		<xsl:apply-templates />
	</xsl:template>

	<xsl:template match="dtb:dl">
		<xsl:apply-templates />
	</xsl:template>

	<xsl:template match="dtb:dl/dtb:dt">
		<xsl:apply-templates />
	</xsl:template>

	<xsl:template match="dtb:w">
		<xsl:apply-templates />
	</xsl:template>

	<!--
	This template copies the attributes that are common to all DTB elements
	-->
	<xsl:template name="copyCommonAttributes">
		<!-- 
		If there is an id on the element then set it to the x:Name attribute.
		This avoid output elements having blank x:Name attributes.
		-->
		<xsl:if test="@id">
			<xsl:attribute name="x:Name">
				<xsl:value-of select="@id"/>
			</xsl:attribute>
		</xsl:if>

		<!-- if no ID is present then make one up. This is important so that all elements can be
		identified in code -->
		<xsl:if test="not(@id)">
			<xsl:attribute name="x:Name">
				<xsl:text>_</xsl:text>
				<xsl:value-of select="generate-id()"/>
			</xsl:attribute>
		</xsl:if>
	</xsl:template>

	<xsl:template match="*[@render = 'optional']">

	</xsl:template>

	<xsl:template name="writeContents">
		<xsl:value-of select="concat(' ', normalize-space(.), ' ')"/>
	</xsl:template>
	<!--
	This template ensure that we don't output any text that isn't part of a recognised
	input element. This does risk not displaying text that should be displayed but will 
	prevent invalid output XAML. Could consider a different approach whereby we ONLY apply 
	templates to known elements.
	-->
	<xsl:template match="text()">
	</xsl:template>

</xsl:stylesheet>
