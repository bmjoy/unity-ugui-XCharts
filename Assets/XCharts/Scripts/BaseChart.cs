﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace xcharts
{
    [System.Serializable]
    public enum ChartType
    {
        line,
        bar
    }

    [System.Serializable]
    public class Title
    {
        public bool show = true;
        public string text;
        public Color color;
        public Align align;
        public float left;
        public float right;
        public float top;
        public float bottom;
    }

    [System.Serializable]
    public class Coordinate
    {
        public bool show = true;
        public float left = 40f;
        public float right = 10f;
        public float top = 10;
        public float bottom = 20f;
        public float tickness = 0.8f;
        public float scaleLen = 5.0f;
    }

    [System.Serializable]
    public enum AxisType
    {
        value,
        category,
        time,
        log
    }

    [System.Serializable]
    public enum Align
    {
        left,                       //左对齐
        right,                      //右对齐
        center                      //居中对齐
    }

    [System.Serializable]
    public enum Location
    {
        left,
        right,
        top,
        bottom,
        start,
        middle,
        center,
        end,
    }

    [System.Serializable]
    public class Axis
    {
        public AxisType type;
        public int splitNumber = 5;
        public int maxSplitNumber = 5;
        public bool showSplitLine;
        public bool boundaryGap = true;
        public List<string> data;

        public void AddCategory(string category)
        {
            if (data.Count >= maxSplitNumber)
            {
                data.RemoveAt(0);
            }
            data.Add(category);
        }
    }

    [System.Serializable]
    public class XAxis : Axis
    {
    }

    [System.Serializable]
    public class YAxis : Axis
    {
    }

    [System.Serializable]
    public class LegendData
    {
        public bool show = true;
        public ChartType type;
        public string key;
        public string text;
        public Color color;
        public Button button { get; set; }
    }

    [System.Serializable]
    public class Legend
    {
        public bool show = true;
        public Location location;
        public float dataWid = 50.0f;
        public float dataHig = 20.0f;
        public float dataSpace;
        public float left;
        public float right;
        public float top;
        public float bottom;
        public List<LegendData> dataList = new List<LegendData>();

        public Color GetColor(int seriesIndex)
        {
            if (seriesIndex < 0 || seriesIndex >= dataList.Count) seriesIndex = 0;
            return dataList[seriesIndex].color;
        }

        public bool IsShowSeries(int seriesIndex)
        {
            if (seriesIndex < 0 || seriesIndex >= dataList.Count) seriesIndex = 0;
            return dataList[seriesIndex].show;
        }
    }

    [System.Serializable]
    public class SeriesData
    {
        public string key;
        public float value;

        public SeriesData(string key, float value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [System.Serializable]
    public class Series
    {
        public string legendKey;
        public int showDataNumber = 0;
        public List<SeriesData> dataList = new List<SeriesData>();

        public float Max
        {
            get
            {
                float max = 0;
                foreach (var data in dataList)
                {
                    if (data.value > max)
                    {
                        max = data.value;
                    }
                }
                return max;
            }
        }

        public float Total
        {
            get
            {
                float total = 0;
                foreach (var data in dataList)
                {
                    total += data.value;
                }
                return total;
            }
        }

        public void AddData(string key, float value)
        {
            if (dataList.Count >= showDataNumber && showDataNumber != 0)
            {
                dataList.RemoveAt(0);
            }
            dataList.Add(new SeriesData(key, value));
        }
    }

    public class BaseChart : MaskableGraphic
    {
        private const int DEFAULT_YSACLE_NUM = 5;
        private const string YSCALE_TEXT_PREFIX = "yScale";
        private const string XSCALE_TEXT_PREFIX = "xScale";
        private const string TILTE_TEXT = "title";
        private const string LEGEND_TEXT = "legend";
        [SerializeField]
        protected Font font;
        [SerializeField]
        protected Color backgroundColor = Color.black;
        [SerializeField]
        protected Title title;
        [SerializeField]
        protected Coordinate coordinate;
        [SerializeField]
        protected XAxis xAxis;
        [SerializeField]
        protected YAxis yAxis;
        [SerializeField]
        protected Legend legend;
        [SerializeField]
        protected List<Series> seriesList = new List<Series>();

        //============check changed=================
        private float lastCoordinateWid;
        private float lastCoordinateHig;
        private float lastCoordinateScaleLen;

        //private Align lastTitleAlign;
        //private float lastTitleLeft;
        //private float lastTitleRight;
        //private float lastTitleTop;

        private float lastXMaxValue;
        private float lastYMaxValue;

        private Coordinate checkCoordinate = new Coordinate();
        private Title checkTitle = new Title();
        private Legend checkLegend = new Legend();
        private XAxis checkXAxis = new XAxis();
        private YAxis checkYAxis = new YAxis();
        //===========================================

        protected Text titleText;
        protected List<Text> yScaleTextList = new List<Text>();
        protected List<Text> xScaleTextList = new List<Text>();
        protected List<Text> legendTextList = new List<Text>();

        protected float zeroX { get { return coordinate.left; } }
        protected float zeroY { get { return coordinate.bottom; } }
        protected float chartWid { get { return rectTransform.sizeDelta.x; } }
        protected float chartHig { get { return rectTransform.sizeDelta.y; } }
        protected float coordinateWid { get { return chartWid - coordinate.left - coordinate.right; } }
        protected float coordinateHig { get { return chartHig - coordinate.top - coordinate.bottom; } }

        protected override void Awake()
        {
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.pivot = Vector2.zero;
            lastCoordinateHig = chartHig;
            lastCoordinateWid = chartWid;
            lastCoordinateScaleLen = coordinate.scaleLen;

            InitTitle();
            InitXScale();
            InitYScale();
            InitLegend();
        }

        protected virtual void Update()
        {
            CheckTile();
            CheckLegend();
            CheckYAxisType();
            CheckXAxisType();
            CheckMaxValue();
            CheckCoordinate();
        }

        public void AddData(string legend, string key, float value)
        {
            for (int i = 0; i < seriesList.Count; i++)
            {
                if (seriesList[i].legendKey == legend)
                {
                    seriesList[i].AddData(key, value);
                    break;
                }
            }
            RefreshChart();
        }

        public void AddXAxisCategory(string category)
        {
            xAxis.AddCategory(category);
            OnXAxisChanged();
        }

        public void AddYAxisCategory(string category)
        {
            yAxis.AddCategory(category);
            OnYAxisChanged();
        }

        protected void HideChild(string match = null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (match == null)
                    transform.GetChild(i).gameObject.SetActive(false);
                else
                {
                    var go = transform.GetChild(i);
                    if (go.name.StartsWith(match))
                    {
                        go.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void InitTitle()
        {
            TextAnchor anchor = TextAnchor.MiddleCenter;
            Vector2 anchorMin = new Vector2(0, 0);
            Vector2 anchorMax = new Vector2(0, 0);
            Vector3 titlePosition;
            float titleWid = 200;
            float titleHig = 20;
            switch (title.align)
            {
                case Align.left:
                    anchor = TextAnchor.MiddleLeft;
                    titlePosition = new Vector3(title.left, chartHig - title.top, 0);
                    break;
                case Align.right:
                    anchor = TextAnchor.MiddleRight;
                    titlePosition = new Vector3(chartWid - title.right - titleWid, chartHig - title.top, 0);
                    break;
                case Align.center:
                    anchor = TextAnchor.MiddleCenter;
                    titlePosition = new Vector3(chartWid / 2 - titleWid / 2, chartHig - title.top, 0);
                    break;
                default:
                    anchor = TextAnchor.MiddleCenter;
                    titlePosition = new Vector3(0, -title.top, 0);
                    break;
            }
            titleText = ChartUtils.AddTextObject(TILTE_TEXT, transform, font,
                        anchor, anchorMin, anchorMax, new Vector2(0, 1),
                        new Vector2(titleWid, titleHig), 16);
            titleText.alignment = anchor;
            titleText.gameObject.SetActive(title.show);
            titleText.transform.localPosition = titlePosition;
            titleText.text = title.text;
        }

        private void InitYScale()
        {
            yScaleTextList.Clear();
            if (yAxis.type == AxisType.value)
            {
                float max = GetMaxValue();
                if (max <= 0) max = 400;
                yAxis.splitNumber = DEFAULT_YSACLE_NUM;
                for (int i = 0; i < yAxis.splitNumber; i++)
                {
                    Text txt = ChartUtils.AddTextObject(YSCALE_TEXT_PREFIX + i, transform, font,
                        TextAnchor.MiddleRight, Vector2.zero, Vector2.zero, new Vector2(1, 0.5f),
                        new Vector2(coordinate.left, 20));
                    txt.transform.localPosition = GetYScalePosition(i);
                    txt.text = ((int)(max * i / yAxis.splitNumber)).ToString();
                    txt.gameObject.SetActive(coordinate.show);
                    yScaleTextList.Add(txt);
                }
            }
            else
            {
                yAxis.splitNumber = yAxis.boundaryGap ? yAxis.data.Count + 1 : yAxis.data.Count;
                for (int i = 0; i < yAxis.data.Count; i++)
                {
                    Text txt = ChartUtils.AddTextObject(YSCALE_TEXT_PREFIX + i, transform, font,
                        TextAnchor.MiddleRight, Vector2.zero, Vector2.zero, new Vector2(1, 0.5f),
                        new Vector2(coordinate.left, 20));
                    txt.transform.localPosition = GetYScalePosition(i);
                    txt.text = yAxis.data[i];
                    txt.gameObject.SetActive(coordinate.show);
                    yScaleTextList.Add(txt);
                }
            }
        }

        private void InitXScale()
        {
            xScaleTextList.Clear();
            if (xAxis.type == AxisType.value)
            {
                float max = GetMaxValue();
                if (max <= 0) max = 400;
                xAxis.splitNumber = DEFAULT_YSACLE_NUM;
                float scaleWid = coordinateWid / (xAxis.splitNumber - 1);
                for (int i = 0; i < xAxis.splitNumber; i++)
                {
                    Text txt = ChartUtils.AddTextObject(XSCALE_TEXT_PREFIX + i, transform, font,
                        TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, new Vector2(1, 0.5f),
                        new Vector2(scaleWid, 20));
                    txt.transform.localPosition = GetXScalePosition(i);
                    txt.text = ((int)(max * i / xAxis.splitNumber)).ToString();
                    txt.gameObject.SetActive(coordinate.show);
                    xScaleTextList.Add(txt);
                }
            }
            else
            {
                xAxis.splitNumber = xAxis.boundaryGap ? xAxis.data.Count + 1 : xAxis.data.Count;
                float scaleWid = coordinateWid / (xAxis.data.Count - 1);
                for (int i = 0; i < xAxis.data.Count; i++)
                {
                    Text txt = ChartUtils.AddTextObject(XSCALE_TEXT_PREFIX + i, transform, font,
                        TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, new Vector2(1, 0.5f),
                        new Vector2(scaleWid, 20));
                    txt.transform.localPosition = GetXScalePosition(i);
                    txt.text = xAxis.data[i];
                    txt.gameObject.SetActive(coordinate.show);
                    xScaleTextList.Add(txt);
                }
            }
        }

        private void InitLegend()
        {
            for (int i = 0; i < legend.dataList.Count; i++)
            {
                LegendData data = legend.dataList[i];
                Button btn = ChartUtils.AddButtonObject(LEGEND_TEXT + i, transform, font, Vector2.zero,
                    Vector2.zero, Vector2.zero, new Vector2(legend.dataWid, legend.dataHig));
                legend.dataList[i].button = btn;
                Color bcolor = data.show ? data.color : Color.grey;
                btn.gameObject.SetActive(legend.show);
                btn.transform.localPosition = GetLegendPosition(i);
                btn.GetComponent<Image>().color = bcolor;
                btn.GetComponentInChildren<Text>().text = data.text;
                btn.onClick.AddListener(delegate ()
                {
                    data.show = !data.show;
                    btn.GetComponent<Image>().color = data.show ? data.color : Color.grey;
                    OnYMaxValueChanged();
                    OnLegendButtonClicked();
                    RefreshChart();
                });
            }
        }

        private Vector3 GetLegendPosition(int i)
        {
            int legendCount = legend.dataList.Count;
            switch (legend.location)
            {
                case Location.bottom:
                case Location.top:
                    float startX = legend.left;
                    if (startX <= 0)
                    {
                        startX = (chartWid - (legendCount * legend.dataWid -
                            (legendCount - 1) * legend.dataSpace)) / 2;
                    }
                    float posY = legend.location == Location.bottom ?
                        legend.bottom : chartHig - legend.top - legend.dataHig;
                    return new Vector3(startX + i * (legend.dataWid + legend.dataSpace), posY, 0);
                case Location.left:
                case Location.right:
                    float startY = 0;
                    if (legend.top > 0)
                    {
                        startY = chartHig - legend.top - legend.dataHig;
                    }
                    else if (startY <= 0)
                    {
                        float offset = (chartHig - (legendCount * legend.dataHig - (legendCount - 1) * legend.dataSpace)) / 2;
                        startY = chartHig - offset - legend.dataHig;
                    }
                    float posX = legend.location == Location.left ? legend.left : chartWid - legend.right - legend.dataWid;
                    return new Vector3(posX, startY - i * (legend.dataHig + legend.dataSpace), 0);
                default: break;
            }
            return Vector3.zero;
        }

        private Vector3 GetYScalePosition(int i)
        {
            float scaleWid = coordinateHig / (yAxis.splitNumber - 1);
            if (yAxis.type == AxisType.value)
            {
                return new Vector3(zeroX - coordinate.scaleLen - 2f,
                zeroY + i * scaleWid, 0);
            }
            else
            {
                if (yAxis.boundaryGap)
                {
                    return new Vector3(zeroX - coordinate.scaleLen - 2f,
                        zeroY + (i + 0.5f) * scaleWid, 0);
                }
                else
                {
                    return new Vector3(zeroX - coordinate.scaleLen - 2f,
                        zeroY + i * scaleWid, 0);
                }

            }
        }

        private Vector3 GetXScalePosition(int i)
        {
            float scaleWid = coordinateWid / (xAxis.splitNumber - 1);
            if (xAxis.type == AxisType.value)
            {
                return new Vector3(zeroX + (i + 1 - 0.5f) * scaleWid, zeroY - coordinate.scaleLen - 10, 0);
            }
            else
            {
                if (xAxis.boundaryGap)
                {
                    return new Vector3(zeroX + (i + 1) * scaleWid, zeroY - coordinate.scaleLen - 5, 0);
                }
                else
                {
                    return new Vector3(zeroX + (i + 1 - 0.5f) * scaleWid, zeroY - coordinate.scaleLen - 5, 0);
                }
            }
        }

        private void CheckCoordinate()
        {
            if (lastCoordinateHig != coordinateHig
                || lastCoordinateWid != coordinateWid
                || lastCoordinateScaleLen != coordinate.scaleLen)
            {
                lastCoordinateWid = coordinateWid;
                lastCoordinateHig = coordinateHig;
                lastCoordinateScaleLen = coordinate.scaleLen;
                OnCoordinateSize();
            }
            if(checkCoordinate.show != coordinate.show)
            {
                checkCoordinate.show = coordinate.show;
                OnXAxisChanged();
                OnYAxisChanged();
            }
        }

        private void CheckYAxisType()
        {
            if (checkYAxis.type != yAxis.type ||
                checkYAxis.boundaryGap != yAxis.boundaryGap ||
                checkYAxis.showSplitLine != yAxis.showSplitLine ||
                checkYAxis.splitNumber != yAxis.splitNumber)
            {
                checkYAxis.type = yAxis.type;
                checkYAxis.boundaryGap = yAxis.boundaryGap;
                checkYAxis.showSplitLine = yAxis.showSplitLine;
                checkYAxis.splitNumber = yAxis.splitNumber;
                OnYAxisChanged();
            }
        }

        private void CheckXAxisType()
        {
            if (checkXAxis.type != xAxis.type ||
                checkXAxis.boundaryGap != xAxis.boundaryGap ||
                checkXAxis.showSplitLine != xAxis.showSplitLine ||
                checkXAxis.splitNumber != xAxis.splitNumber)
            {
                checkXAxis.type = xAxis.type;
                checkXAxis.boundaryGap = xAxis.boundaryGap;
                checkXAxis.showSplitLine = xAxis.showSplitLine;
                checkXAxis.splitNumber = xAxis.splitNumber;
                OnXAxisChanged();
            }
        }

        private void CheckMaxValue()
        {
            if (xAxis.type == AxisType.value)
            {
                float max = GetMaxValue();
                if (lastXMaxValue != max)
                {
                    lastXMaxValue = max;
                    OnXMaxValueChanged();
                }
            }
            else if (yAxis.type == AxisType.value)
            {

                float max = GetMaxValue();
                if (lastYMaxValue != max)
                {
                    lastYMaxValue = max;
                    OnYMaxValueChanged();
                }
            }
        }

        protected float GetMaxValue()
        {
            float max = 0;
            for (int i = 0; i < seriesList.Count; i++)
            {
                if (legend.IsShowSeries(i) && seriesList[i].Max > max) max = seriesList[i].Max;
            }
            int bigger = (int)(max * 1.3f);
            return bigger < 10 ? bigger : bigger - bigger % 10;
        }

        private void CheckTile()
        {
            if (checkTitle.align != title.align ||
                checkTitle.left != title.left ||
                checkTitle.right != title.right ||
                checkTitle.top != title.top)
            {
                checkTitle.align = title.align;
                checkTitle.left = title.left;
                checkTitle.right = title.right;
                checkTitle.top = title.top;
                OnTitleChanged();
            }
        }

        private void CheckLegend()
        {
            if (checkLegend.dataWid != legend.dataWid ||
                checkLegend.dataHig != legend.dataHig ||
                checkLegend.dataSpace != legend.dataSpace ||
                checkLegend.left != legend.left ||
                checkLegend.right != legend.right ||
                checkLegend.bottom != legend.bottom ||
                checkLegend.top != legend.top ||
                checkLegend.location != legend.location ||
                checkLegend.show != legend.show)
            {
                checkLegend.dataWid = legend.dataWid;
                checkLegend.dataHig = legend.dataHig;
                checkLegend.dataSpace = legend.dataSpace;
                checkLegend.left = legend.left;
                checkLegend.right = legend.right;
                checkLegend.bottom = legend.bottom;
                checkLegend.top = legend.top;
                checkLegend.location = legend.location;
                checkLegend.show = legend.show;
                OnLegendChanged();
            }
        }

        protected virtual void OnCoordinateSize()
        {
            //update yScale pos
            for (int i = 0; i < yAxis.splitNumber; i++)
            {
                if (i < yScaleTextList.Count && yScaleTextList[i])
                {
                    yScaleTextList[i].transform.localPosition = GetYScalePosition(i);
                }
            }
            for (int i = 0; i < xAxis.splitNumber; i++)
            {
                if (i < xScaleTextList.Count && xScaleTextList[i])
                {
                    xScaleTextList[i].transform.localPosition = GetXScalePosition(i);
                }
            }
        }

        protected virtual void OnYAxisChanged()
        {
            HideChild(YSCALE_TEXT_PREFIX);
            InitYScale();
        }

        protected virtual void OnXAxisChanged()
        {
            HideChild(XSCALE_TEXT_PREFIX);
            InitXScale();
        }

        protected virtual void OnXMaxValueChanged()
        {
            float max = GetMaxValue();
            for (int i = 0; i < xScaleTextList.Count; i++)
            {
                xScaleTextList[i].text = ((int)(max * i / xScaleTextList.Count)).ToString();
            }
        }

        protected virtual void OnYMaxValueChanged()
        {
            float max = GetMaxValue();
            for (int i = 0; i < yScaleTextList.Count; i++)
            {
                yScaleTextList[i].text = ((int)(max * i / (yScaleTextList.Count - 1))).ToString();
            }
        }

        protected virtual void OnTitleChanged()
        {
            InitTitle();
        }

        protected virtual void OnLegendChanged()
        {
            for (int i = 0; i < legend.dataList.Count; i++)
            {
                Button btn = legend.dataList[i].button;
                btn.GetComponent<RectTransform>().sizeDelta = new Vector2(legend.dataWid, legend.dataHig);
                Text txt = btn.GetComponentInChildren<Text>();
                txt.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(legend.dataWid, legend.dataHig);
                txt.transform.localPosition = Vector3.zero;
                btn.transform.localPosition = GetLegendPosition(i);
                btn.gameObject.SetActive(legend.show);
            }
        }

        protected virtual void OnLegendButtonClicked()
        {

        }

        protected void RefreshChart()
        {
            int tempWid = (int)chartWid;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tempWid - 1);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tempWid);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            DrawBackground(vh);
            DrawCoordinate(vh);
        }

        private void DrawBackground(VertexHelper vh)
        {
            // draw bg
            Vector3 p1 = new Vector3(0, chartHig);
            Vector3 p2 = new Vector3(chartWid, chartHig);
            Vector3 p3 = new Vector3(chartWid, 0);
            Vector3 p4 = new Vector3(0, 0);
            ChartUtils.DrawPolygon(vh, p1, p2, p3, p4, backgroundColor);
        }

        private void DrawCoordinate(VertexHelper vh)
        {
            if (!coordinate.show) return;
            // draw scale
            for (int i = 1; i < yAxis.splitNumber; i++)
            {
                float pX = zeroX - coordinate.scaleLen;
                float pY = zeroY + i * coordinateHig / (yAxis.splitNumber - 1);
                ChartUtils.DrawLine(vh, new Vector3(pX, pY), new Vector3(zeroX, pY), coordinate.tickness,
                    Color.white);
                if (yAxis.showSplitLine)
                {
                    ChartUtils.DrawLine(vh, new Vector3(zeroX, pY),
                        new Vector3(zeroX + coordinateWid, pY), coordinate.tickness, Color.grey);
                }
            }
            for (int i = 1; i < xAxis.splitNumber; i++)
            {
                float pX = zeroX + i * coordinateWid / (xAxis.splitNumber - 1);
                float pY = zeroY - coordinate.scaleLen - 2;
                ChartUtils.DrawLine(vh, new Vector3(pX, zeroY), new Vector3(pX, pY), coordinate.tickness,
                    Color.white);
                if (xAxis.showSplitLine)
                {
                    ChartUtils.DrawLine(vh, new Vector3(pX, zeroY), new Vector3(pX, zeroY + coordinateHig),
                        coordinate.tickness, Color.grey);
                }
            }
            //draw x,y axis
            ChartUtils.DrawLine(vh, new Vector3(zeroX, zeroY - coordinate.scaleLen),
                new Vector3(zeroX, zeroY + coordinateHig + 2), coordinate.tickness, Color.white);
            ChartUtils.DrawLine(vh, new Vector3(zeroX - coordinate.scaleLen, zeroY),
                new Vector3(zeroX + coordinateWid + 2, zeroY), coordinate.tickness, Color.white);
        }
    }
}