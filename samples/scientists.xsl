<?xml version="1.0" encoding="utf-8"?>
<!-- Перетворює список науковців (вхідний або відфільтрований XML) на HTML-таблицю -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes" />

  <xsl:template match="/Scientists">
    <html>
      <head>
        <title>Кадри науковців (зарплата)</title>
        <style>
          body { font-family: Segoe UI, sans-serif; margin: 24px; }
          table { border-collapse: collapse; }
          th, td { border: 1px solid #999; padding: 4px 10px; }
          th { background: #e8e4f8; }
          td.number { text-align: right; }
        </style>
      </head>
      <body>
        <h2>Кадри науковців (зарплата)</h2>
        <p>Знайдено записів: <xsl:value-of select="count(Scientist)" /></p>
        <table>
          <tr>
            <th>ПІП</th>
            <th>Факультет</th>
            <th>Кафедра</th>
            <th>Посада</th>
            <th>Оклад, грн</th>
            <th>На посаді з</th>
            <th>Науковий ступінь</th>
          </tr>
          <xsl:for-each select="Scientist">
            <tr>
              <td><xsl:value-of select="@FullName" /></td>
              <td><xsl:value-of select="@Faculty" /></td>
              <td><xsl:value-of select="@Department" /></td>
              <td><xsl:value-of select="@Position" /></td>
              <td class="number"><xsl:value-of select="@Salary" /></td>
              <td class="number"><xsl:value-of select="@Since" /></td>
              <td><xsl:value-of select="@Degree" /></td>
            </tr>
          </xsl:for-each>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
