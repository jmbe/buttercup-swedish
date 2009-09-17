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
  xmlns=""
  exclude-result-prefixes="dtb s m svg xs">

	<xsl:param name="filter_word"/>
	<xsl:param name="baseDir"/>
	<xsl:param name="first_smil"/>
	<xsl:param name="css_path"/>
	<xsl:param name="daisy_noteref"/>
	<xsl:param name="svg_mathml"/>
	<xsl:param name="split_simple_table"/>

	<xsl:output method="html" encoding="utf-8" indent="yes" omit-xml-declaration = "yes"/>

	<!-- <!ENTITY catts "@id|@class|@title|@xml:lang"> -->
	<xsl:template name="copyCatts">
		<xsl:copy-of select="@id|@class|@title"/>
	</xsl:template>

	<!-- <!ENTITY catts "@id|@class|@title|@xml:lang"> -->
	<xsl:template name="copyObjatts">
		<xsl:copy-of select="@id|@type|@name|@value"/>
	</xsl:template>

	<!-- <!ENTITY cncatts "@id|@title|@xml:lang"> -->
	<xsl:template name="copyCncatts">
		<xsl:copy-of select="@id|@title|@xml:lang"/>
	</xsl:template>

	<xsl:template name="copyAttsNoId">
		<xsl:copy-of select="@class|@title|@xml:lang"/>
	</xsl:template>

	<!-- <!ENTITY inlineParent "ancestor::*[self::dtb:h1 or self::dtb:h2 or self::dtb:h3 or self::dtb:h4 or self::dtb:h5 or self::dtb:h6 or self::dtb:hd or self::dtb:span or self::dtb:p]"> -->
	<xsl:template name="inlineParent">
		<xsl:param name="class"/>
		<xsl:choose>
			<xsl:when test="ancestor::*[self::dtb:h1 or self::dtb:h2 or self::dtb:h3 or self::dtb:h4 or self::dtb:h5 or self::dtb:h6 or self::dtb:hd or self::dtb:span or self::dtb:p or self::dtb:lic]">
				<xsl:apply-templates select="." mode="inlineOnly"/>
			</xsl:when>
			<!-- jpritchett@rfbd.org:  Fixed bug in setting @class value (missing braces) -->
			<xsl:otherwise>
				<div class="{$class}">
					<xsl:call-template name="copyCncatts"/>
					<xsl:apply-templates/>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>


	<xsl:template match="dtb:dtbook">
		<div>
		<xsl:apply-templates/>
		</div>
	</xsl:template>




	<xsl:template match="dtb:book">
		<xsl:for-each select="(//dtb:doctitle)[1]">
			<h1>
				<xsl:call-template name="copyCatts"/>
				<xsl:apply-templates/>
			</h1>
		</xsl:for-each>
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="dtb:frontmatter|dtb:bodymatter|dtb:rearmatter">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="dtb:level1">
		<div>
			<xsl:call-template name="copyCatts"/>
			<xsl:if test="not(@class)">
				<xsl:attribute name="class">level1</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="dtb:level2">
		<div>
			<xsl:call-template name="copyCatts"/>
			<xsl:if test="not(@class)">
				<xsl:attribute name="class">level2</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="dtb:level3">
		<div>
			<xsl:call-template name="copyCatts"/>
			<xsl:if test="not(@class)">
				<xsl:attribute name="class">level3</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="dtb:level4">
		<div>
			<xsl:call-template name="copyCatts"/>
			<xsl:if test="not(@class)">
				<xsl:attribute name="class">level4</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="dtb:level5">
		<div>
			<xsl:call-template name="copyCatts"/>
			<xsl:if test="not(@class)">
				<xsl:attribute name="class">level5</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="dtb:level6">
		<div>
			<xsl:call-template name="copyCatts"/>
			<xsl:if test="not(@class)">
				<xsl:attribute name="class">level6</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="dtb:level">
		<div>
			<xsl:call-template name="copyCatts"/>
			<xsl:if test="not(@class)">
				<xsl:attribute name="class">level</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates/>
		</div>
	</xsl:template>


	<xsl:template match="dtb:covertitle">
		<p>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates mode="inlineOnly"/>
		</p>
	</xsl:template>



	<xsl:template match="dtb:p">
		<p>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates mode="inlineOnly"/>
		</p>
	</xsl:template>


	<xsl:template name="pagenum">
		<span class="pagenum">
			<xsl:call-template name="copyCncatts"/>
			<xsl:choose>
				<xsl:when test="@page='front'">
					<xsl:attribute name="class">page-front</xsl:attribute>
				</xsl:when>
				<xsl:when test="@page='special'">
					<xsl:attribute name="class">page-special</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="class">page-normal</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:call-template name="maybeSmilref"/>
			<!--<xsl:apply-templates/>-->
		</span>
	</xsl:template>

	<xsl:template match="dtb:pagenum">
		<xsl:call-template name="pagenum"/>
	</xsl:template>

	<xsl:template match="dtb:list/dtb:pagenum" priority="1">
		<xsl:param name="inlineFix"/>
		<xsl:choose>
			<xsl:when test="not(preceding-sibling::*) or $inlineFix='true'">
				<li>
					<xsl:call-template name="pagenum"/>
				</li>
			</xsl:when>
			<xsl:otherwise>
				<!--<xsl:message>Skipping pagenum element <xsl:value-of select="@id"/></xsl:message>-->
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="dtb:list/dtb:pagenum" mode="pagenumInLi">
		<xsl:call-template name="pagenum"/>
		<xsl:apply-templates select="following-sibling::*[1][self::dtb:pagenum]" mode="pagenumInLi"/>
	</xsl:template>



	<xsl:template match="dtb:list/dtb:prodnote">
		<li class="optional-prodnote">
			<xsl:apply-templates/>
		</li>
	</xsl:template>

	<xsl:template match="dtb:blockquote/dtb:pagenum">
		<div class="dummy">
			<xsl:call-template name="pagenum"/>
		</div>
	</xsl:template>

	<xsl:template match="dtb:address">
		<div class="address">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>


	<xsl:template match="dtb:h1">
		<h1>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</h1>
	</xsl:template>

	<xsl:template match="dtb:h2">
		<h2>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</h2>
	</xsl:template>

	<xsl:template match="dtb:h3">
		<h3>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</h3>
	</xsl:template>

	<xsl:template match="dtb:h4">
		<h4>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</h4>
	</xsl:template>

	<xsl:template match="dtb:h5">
		<h5>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</h5>
	</xsl:template>

	<xsl:template match="dtb:h6">
		<h6>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</h6>
	</xsl:template>


	<xsl:template match="dtb:bridgehead">
		<div class="bridgehead">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>




	<xsl:template match="dtb:list[not(@type)]">
		<ul>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</ul>
	</xsl:template>





	<xsl:template match="dtb:lic">
		<span class="lic">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</span>
	</xsl:template>

	<xsl:template match="dtb:br">
		<br/>
	</xsl:template>


	<xsl:template match="dtb:noteref">
		<xsl:choose>
			<xsl:when test="$daisy_noteref='true'">
				<span class="noteref">
					<xsl:call-template name="copyCncatts"/>
					<xsl:attribute name="bodyref">
						<xsl:if test="not(contains(@idref,'#'))">
							<xsl:text>#</xsl:text>
						</xsl:if>
						<xsl:value-of select="@idref"/>
					</xsl:attribute>
					<xsl:apply-templates/>
				</span>
			</xsl:when>
			<xsl:otherwise>
				<a class="noteref">
					<xsl:call-template name="copyCncatts"/>
					<xsl:attribute name="href">
						<xsl:choose>
							<xsl:when test="@smilref">
								<xsl:value-of select="@smilref"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:text>#</xsl:text>
								<xsl:value-of select="translate(@idref, '#', '')"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:attribute>
					<xsl:apply-templates/>
				</a>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>


	<xsl:template match="dtb:img">
		<img>
			<xsl:call-template name="copyCatts"/>
			<xsl:copy-of select="@src|@alt|@longdesc|@height|@width"/>
		</img>
	</xsl:template>


	<xsl:template match="dtb:caption">
		<caption>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates mode="inlineOnly"/>
		</caption>
	</xsl:template>


	<xsl:template match="dtb:imggroup/dtb:caption">
		<div class="caption">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="dtb:div">
		<div>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="dtb:imggroup">
		<xsl:call-template name="inlineParent">
			<xsl:with-param name="class" select="'imggroup'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="dtb:prodnote">
		<xsl:call-template name="inlineParent">
			<xsl:with-param name="class" select="'optional-prodnote'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="dtb:annotation">
		<div class="annotation">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="dtb:author">
		<div class="author">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="dtb:blockquote">
		<blockquote>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</blockquote>
	</xsl:template>


	<xsl:template match="dtb:byline">
		<div class="byline">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="dtb:dateline">
		<div class="dateline">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="dtb:docauthor">
		<div class="docauthor">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="dtb:epigraph">
		<div class="epigraph">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="dtb:note">
		<div class="notebody">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="dtb:sidebar">
		<div class="sidebar">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="dtb:hd">
		<xsl:choose>
			<xsl:when test="parent::dtb:level">
				<xsl:element name="{concat('h', count(ancestor::dtb:level))}">
					<xsl:call-template name="copyCatts"/>
					<xsl:apply-templates/>
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<div class="hd">
					<xsl:call-template name="copyCncatts"/>
					<xsl:apply-templates/>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="dtb:list/dtb:hd">
		<li class="hd">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
			<xsl:apply-templates select="following-sibling::*[1][self::dtb:pagenum]" mode="pagenumInLi"/>
		</li>
	</xsl:template>




	<xsl:template match="dtb:list[@type='ol']">
		<ol>
			<xsl:choose>
				<xsl:when test="@enum='i'">
					<xsl:attribute name="class">lower-roman</xsl:attribute>
				</xsl:when>
				<xsl:when test="@enum='I'">
					<xsl:attribute name="class">upper-roman</xsl:attribute>
				</xsl:when>
				<xsl:when test="@enum='a'">
					<xsl:attribute name="class">lower-alpha</xsl:attribute>
				</xsl:when>
				<xsl:when test="@enum='A'">
					<xsl:attribute name="class">upper-alpha</xsl:attribute>
				</xsl:when>
			</xsl:choose>
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</ol>
	</xsl:template>





	<xsl:template match="dtb:list[@type='ul']">
		<ul>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</ul>
	</xsl:template>

	<xsl:template match="dtb:list[@type='pl']">
		<ul class="plain">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</ul>
	</xsl:template>

	<xsl:template match="dtb:li">
		<li>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
			<xsl:apply-templates select="following-sibling::*[1][self::dtb:pagenum]" mode="pagenumInLi"/>
		</li>
	</xsl:template>



	<xsl:template match="dtb:dl">
		<dl>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</dl>
	</xsl:template>

	<xsl:template match="dtb:dl/dtb:pagenum" priority="1">
		<dt>
			<xsl:call-template name="pagenum"/>
		</dt>
		<dd>
			<xsl:comment>empty</xsl:comment>
		</dd>
	</xsl:template>

	<xsl:template match="dtb:dt">
		<dt>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</dt>
	</xsl:template>

	<xsl:template match="dtb:dd">
		<dd>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</dd>
	</xsl:template>


	<xsl:template match="dtb:table">
		<xsl:choose>
			<xsl:when test="count(dtb:*[local-name()!='tr' and local-name()!='pagenum' and local-name()!='caption'])=0 and
			                element-available('xsl:for-each-group') and
			                $split_simple_table='true'">
				<xsl:call-template name="simpleTable"/>
			</xsl:when>
			<xsl:otherwise>
				<table>
					<xsl:call-template name="copyCatts"/>
					<xsl:apply-templates/>
				</table>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="simpleTable">
		<xsl:variable name="tableAtts">
			<dummy>
				<xsl:call-template name="copyAttsNoId"/>
			</dummy>
		</xsl:variable>
		<xsl:variable name="idAtt">
			<dummy>
				<xsl:copy-of select="@id"/>
			</dummy>
		</xsl:variable>
		<xsl:variable name="elemName" as="xs:string" select="name()"/>
		<xsl:for-each-group select="node()" group-starting-with="dtb:pagenum">
			<xsl:apply-templates select="current-group()[self::dtb:pagenum]" mode="pagenumonly"/>
			<xsl:if test="current-group()[not(self::dtb:pagenum)]">
				<xsl:element name="{$elemName}">
					<xsl:if test="position()=1">
						<xsl:copy-of select="$idAtt/*/@*"/>
					</xsl:if>
					<xsl:copy-of select="$tableAtts/*/@*"/>

					<xsl:apply-templates select="current-group()[not(self::dtb:pagenum)]"/>
				</xsl:element>
			</xsl:if>
		</xsl:for-each-group>
	</xsl:template>

	<xsl:template match="dtb:pagenum" mode="pagenumonly">
		<xsl:call-template name="pagenum"/>
	</xsl:template>

	<xsl:template match="dtb:table/dtb:pagenum|dtb:tbody/dtb:pagenum">
		<tr>
			<td class="noborder">
				<xsl:attribute name="colspan">
					<xsl:variable name="tdsInRow" select="number(sum(ancestor::dtb:table[1]/descendant::*[self::dtb:td or self::dtb:th]/(@colspan * @rowspan))) div count(ancestor::dtb:table[1]/descendant::dtb:tr)"/>
					<!-- <xsl:message>tdsInRow:<xsl:value-of select="$tdsInRow"/></xsl:message> -->
					<xsl:if test="$tdsInRow != round($tdsInRow) and $tdsInRow != NaN">
						<xsl:message>Warning: Colspan and rowspan values in table don't add up.</xsl:message>
					</xsl:if>
					<xsl:value-of select="floor($tdsInRow)"/>
				</xsl:attribute>
				<xsl:call-template name="pagenum"/>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="dtb:tbody">
		<tbody>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</tbody>
	</xsl:template>



	<xsl:template match="dtb:thead">
		<thead>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</thead>
	</xsl:template>

	<xsl:template match="dtb:tfoot">
		<tfoot>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</tfoot>
	</xsl:template>

	<xsl:template match="dtb:tr">
		<tr>
			<xsl:call-template name="copyCatts"/>
			<xsl:copy-of select="@rowspan|@colspan"/>
			<xsl:apply-templates/>
		</tr>
	</xsl:template>

	<xsl:template match="dtb:th">
		<th>
			<xsl:call-template name="copyCatts"/>
			<xsl:copy-of select="@rowspan|@colspan"/>
			<xsl:apply-templates/>
		</th>
	</xsl:template>

	<xsl:template match="dtb:td">
		<td>
			<xsl:call-template name="copyCatts"/>
			<xsl:copy-of select="@rowspan|@colspan"/>
			<xsl:apply-templates/>
		</td>
	</xsl:template>

	<xsl:template match="dtb:colgroup">
		<colgroup>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</colgroup>
	</xsl:template>

	<xsl:template match="dtb:col">
		<col>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</col>
	</xsl:template>








	<xsl:template match="dtb:poem">
		<div class="poem">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>


	<xsl:template match="dtb:poem/dtb:title">
		<p class="title">
			<xsl:apply-templates/>
		</p>

	</xsl:template>

	<xsl:template match="dtb:cite/dtb:title">
		<span class="title">
			<xsl:apply-templates/>
		</span>

	</xsl:template>

	<xsl:template match="dtb:cite/dtb:author">
		<span class="author">
			<xsl:apply-templates/>
		</span>
	</xsl:template>

	<xsl:template match="dtb:cite">
		<cite>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</cite>
	</xsl:template>



	<xsl:template match="dtb:code">
		<code>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</code>
	</xsl:template>

	<xsl:template match="dtb:kbd">
		<kbd>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</kbd>
	</xsl:template>

	<xsl:template match="dtb:q">
		<q>
			<xsl:call-template name="copyCatts"/>
			<xsl:call-template name="maybeSmilref"/>
			<!--<xsl:apply-templates/>-->
		</q>
	</xsl:template>

	<xsl:template match="dtb:samp">
		<samp>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</samp>
	</xsl:template>



	<xsl:template match="dtb:linegroup">
		<div class="linegroup">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</div>
	</xsl:template>


	<xsl:template match="dtb:line">
		<p class="line">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates mode="inlineOnly"/>
		</p>
	</xsl:template>

	<xsl:template match="dtb:linenum">
		<span class="linenum">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</span>
	</xsl:template>







	<!-- Inlines -->

	<xsl:template match="dtb:a">
		<span class="anchor">
			<xsl:apply-templates/>
		</span>
	</xsl:template>

	<xsl:template match="dtb:em">
		<em>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</em>
	</xsl:template>

	<xsl:template match="dtb:strong">
		<strong>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</strong>
	</xsl:template>


	<xsl:template match="dtb:abbr">
		<abbr>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</abbr>
	</xsl:template>

	<xsl:template match="dtb:acronym">
		<acronym>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</acronym>
	</xsl:template>

	<xsl:template match="dtb:bdo">
		<bdo>
			<xsl:copy-of select="@*"/>
			<xsl:apply-templates/>
		</bdo>
	</xsl:template>

	<xsl:template match="dtb:dfn">
		<span class="definition">
			<xsl:call-template name="copyCncatts"/>
			<xsl:apply-templates/>
		</span>
	</xsl:template>

	<xsl:template match="dtb:sent">
		<span class="sentence">
			<xsl:call-template name="copyCncatts"/>
			<xsl:call-template name="maybeSmilref"/>
		</span>
	</xsl:template>


	<xsl:template match="dtb:w">
		<xsl:choose>
			<xsl:when test="$filter_word='yes'">
				<xsl:apply-templates/>
			</xsl:when>
			<xsl:otherwise>
				<span class="word">
					<xsl:apply-templates/>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>




	<xsl:template match="dtb:sup">
		<sup>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</sup>
	</xsl:template>

	<xsl:template match="dtb:sub">
		<sub>
			<xsl:call-template name="copyCatts"/>
			<xsl:apply-templates/>
		</sub>
	</xsl:template>


	<xsl:template match="dtb:span">
		<span>
			<xsl:call-template name="copyCatts"/>
			<xsl:call-template name="maybeSmilref"/>
		</span>
	</xsl:template>


	<!-- FIXME internal and external -->
	<xsl:template match="dtb:a[@href]">
		<xsl:choose>
			<xsl:when test="ancestor::dtb:*[@smilref]">
				<span class="anchor">
					<xsl:call-template name="copyCncatts"/>
					<xsl:apply-templates/>
				</span>
			</xsl:when>
			<xsl:when test="@smilref">
				<xsl:variable name="url" select="substring-before(@smilref, '#')"/>
				<xsl:variable name="fragment" select="substring-after(@smilref, '#')"/>
				<xsl:choose>
					<xsl:when test="document(concat($baseDir, $url))//*[@id=$fragment and self::s:par] and not(ancestor::dtb:note) and not(descendant::*[@smilref])">
						<a id="{@id}">
							<xsl:attribute name="href">
								<xsl:value-of select="@smilref"/>
							</xsl:attribute>
							<xsl:apply-templates/>
						</a>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<a>
					<xsl:call-template name="copyCatts"/>
					<xsl:copy-of select="@href"/>
					<xsl:apply-templates/>
				</a>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="dtb:annoref">
		<a class="annoref">
			<xsl:call-template name="copyCncatts"/>
			<xsl:attribute name="href">
				<xsl:choose>
					<xsl:when test="@smilref">
						<xsl:value-of select="@smilref"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>#</xsl:text>
						<xsl:value-of select="translate(@idref, '#', '')"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:apply-templates/>
		</a>
	</xsl:template>

	<xsl:template match="dtb:*">
		<xsl:message>
			*****<xsl:value-of select="name(..)"/>/{<xsl:value-of select="namespace-uri()"/>}<xsl:value-of select="name()"/>******
		</xsl:message>
	</xsl:template>


	<xsl:template name="maybeSmilref">
		<xsl:apply-templates/>
	</xsl:template>

	<!--   <!ENTITY isInline "self::dtb:a or self::dtb:em or self::dtb:strong or self::dtb:abbr or self::dtb:acronym or self::dtb:bdo or self::dtb:dfn or self::dtb:sent or self::dtb:w or self::dtb:sup or self::dtb:sub or self::dtb:span or self::dtb:annoref or self::dtb:noteref or self::dtb:img or self::dtb:br or self::dtb:q or self::dtb:samp or self::dtb:pagenum"> -->
	<xsl:template match="dtb:*" mode="inlineOnly">
		<xsl:choose>
			<!-- Tokes: for now simply render img/imggroup according to their templates only -->
			<xsl:when test="self::dtb:img or self::dtb:imggroup">
				<xsl:apply-templates />
			</xsl:when>
			<xsl:when test="self::dtb:a or self::dtb:em or self::dtb:strong or self::dtb:abbr or self::dtb:acronym or self::dtb:bdo or self::dtb:dfn or self::dtb:w or self::dtb:sup or self::dtb:sub or self::dtb:span or self::dtb:annoref or self::dtb:noteref or self::dtb:br or self::dtb:q or self::dtb:img or self::dtb:samp or self::dtb:pagenum">
				<xsl:apply-templates select=".">
					<xsl:with-param name="inlineFix" select="'true'"/>
				</xsl:apply-templates>
			</xsl:when>
			<xsl:otherwise>
				<span>
					<xsl:call-template name="get_class_attribute">
						<xsl:with-param name="element" select="."/>
					</xsl:call-template>
					<xsl:call-template name="copyCncatts"/>
					<xsl:apply-templates mode="inlineOnly"/>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="get_class_attribute">
		<xsl:param name="element"/>
		<xsl:choose>
			<xsl:when test="name($element)='sent'">
			</xsl:when>
			<xsl:when test="name($element)='imggroup'">
				<xsl:attribute name="class">imggroup</xsl:attribute>
			</xsl:when>
			<xsl:when test="name($element)='caption'">
				<xsl:attribute name="class">caption</xsl:attribute>
			</xsl:when>
			<xsl:when test="$element/@class">
				<xsl:attribute name="class">
					<xsl:value-of select="$element/@class"/>
				</xsl:attribute>
			</xsl:when>
			<xsl:otherwise>
				<xsl:attribute name="class">
					<xsl:text>inline-</xsl:text>
					<xsl:value-of select="name($element)"/>
				</xsl:attribute>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>


	<!-- MathML; deep copy -->
	<xsl:template match="m:math">
		<xsl:copy-of select="."/>
	</xsl:template>

	<xsl:template match="m:math" mode="inlineOnly" >
		<xsl:copy-of select="."/>
	</xsl:template>

	<!-- SVG; deep copy -->
	<xsl:template match="svg:svg">
		<xsl:copy-of select="."/>
	</xsl:template>

	<xsl:template match="svg:svg" mode="inlineOnly" >
		<xsl:copy-of select="."/>
	</xsl:template>

	<xsl:template match="dtb:object">
		<object>
			<xsl:call-template name="copyObjatts"/>
			<xsl:apply-templates/>
		</object>
	</xsl:template>

	<xsl:template match="dtb:param">
		<param>
			<xsl:call-template name="copyObjatts"/>
		</param>
	</xsl:template>
</xsl:stylesheet>
