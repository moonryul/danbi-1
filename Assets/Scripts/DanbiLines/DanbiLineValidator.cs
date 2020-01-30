using UnityEngine;
using static DanbiLine.ePosition;
using LinesVector_t = System.Collections.Generic.List<DanbiLine>;

public class DanbiLineValidator : MonoBehaviour {
  public DanbiSpaceAttr Space;
  public DanbiProjectorAttr Projector;
  public DanbiPyramidAttr Pyramid;
  LinesVector_t Lines = new LinesVector_t();
  public LinesVector_t lines {
    get {
      if (Lines.GetEnumerator().Current != null) {
        InitLine(DanbiLine.sLINE_COUNTER++);
      }
      return Lines;
    }
  }


  void InitLine(int idx) {
    if (DanbiLine.sLINE_NUM < DanbiLine.sLINE_COUNTER) {
      ++DanbiLine.sLINE_NUM;
    }
    ++DanbiLine.sLINE_COUNTER;
    Lines.Add(new DanbiLine());
    // 라인을 게임 오브젝트로 초기화.
    var go = new GameObject($"Line <{idx}>").AddComponent<LineRenderer>();
    var newLine = go.GetComponent<LineRenderer>();
    // 라인에 사용할 머터리얼을 Diffuse 로 설정.
    newLine.material = new Material(Shader.Find("Yoonsang/ColoredLine"));
    newLine.material.EnableKeyword("BLACK_ON");
    newLine.material.DisableKeyword("BLUE_ON");
    newLine.material.DisableKeyword("RED_ON");
    // vertexCount = 2 (Deprecated 라서 positionCount Property 사용).
    newLine.positionCount = 2;
    // 너비 설정.
    newLine.startWidth = 1.0f;
    newLine.endWidth = 1.0f;
    // 색상 설정.
    newLine.startColor = Color.black;
    newLine.endColor = Color.black;

    newLine.useWorldSpace = true;
    Lines[idx].line = newLine;
  }

  void Start() {
    // 라인 초기화.
    for (int i = 0; i < DanbiLine.sLINE_NUM; ++i) {
      InitLine(i);
    }

    Space.TopMiddle = new Vector3(0.0f, Space.H, Space.D * 0.5f);
    Pyramid.Apex = Space.TopMiddle - new Vector3(0.0f, Projector.H + Projector.Dist, 0.0f);
    Pyramid.Origin = Space.TopMiddle - new Vector3(0.0f, Projector.H + Projector.Dist + Pyramid.H, 0.0f);
    Projector.BottomMiddle = Space.TopMiddle - new Vector3(0.0f, Projector.H, 0.0f);
  }

  void Update() {
    //ch = R * (-h * h + w * w) / (2 * h * w) - x - y;
    float ch = Space.W * (-Pyramid.H * Pyramid.H + Pyramid.W * Pyramid.W) / (2 * Pyramid.H * Pyramid.W) - Projector.H - Projector.W;
    Debug.Log($"Low part of the target height is {ch}");

    //float k = w * y / Mathf.Sqrt((y + h) * (y + h) + w * w);
    float k = Pyramid.W * Projector.Dist / Mathf.Sqrt((Projector.Dist + Pyramid.H) * (Projector.Dist + Pyramid.H) + Pyramid.W * Pyramid.W);
    //float a = k * k - w * w;
    float a = k * k - Pyramid.W * Pyramid.W;
    //float b = -h * w * (R - w);
    float b = -Pyramid.H * Pyramid.W * (Space.W - Pyramid.W);
    //float c = (R - w) * (R - w) * (k * k - h * h);
    float c = (Space.W - Pyramid.W) * (Space.W - Pyramid.W) * (k * k - Pyramid.H * Pyramid.H);

    float cl = -Projector.H - Projector.Dist - Pyramid.H + (-b - Mathf.Sqrt(b * b - a * c)) / a;
    Debug.Log($"High part of the target height is {cl}");

    // 그리기.
    // 전체 공간 그리기.
    //SetLinePos(0, 0, -Space.W, 0.0f);
    //SetLinePos(0, 1, Space.W, 0.0f);
    //
    //SetLinePos(1, 0, Space.W, 0.0f);
    //SetLinePos(1, 1, Space.W, -Space.H);
    //
    //SetLinePos(2, 0, Space.W, -Space.H);
    //SetLinePos(2, 1, -Space.W, -Space.H);
    //
    //SetLinePos(3, 0, -Space.W, -Space.H);
    //SetLinePos(3, 1, -Space.W, 0.0f);
    //
    //SetLinePos(4, 0, -Projector.W, 0.0f);
    //SetLinePos(4, 1, Projector.W, 0.0f);
    //
    //SetLinePos(5, 0, Projector.W, 0.0f);
    //SetLinePos(5, 1, Projector.W, -Projector.H);
    //
    //SetLinePos(6, 0, Projector.W, -Projector.H);
    //SetLinePos(6, 1, -Projector.W, -Projector.H);
    //
    //SetLinePos(7, 0, -Projector.W, -Projector.H);
    //SetLinePos(7, 1, -Projector.W, 0.0f);
    //
    //SetLinePos(8, 0, 0.0f, -Projector.H - Projector.Dist);
    //SetLinePos(8, 1, Pyramid.W, -Projector.H - Projector.Dist - Pyramid.H);
    //
    //SetLinePos(9, 0, Pyramid.W, -Projector.H - Projector.Dist - Pyramid.H);
    //SetLinePos(9, 1, -Pyramid.W, -Projector.H - Projector.Dist - Pyramid.H);
    //
    //SetLinePos(10, 0, -Pyramid.W, -Projector.H - Projector.Dist - Pyramid.H);
    //SetLinePos(10, 1, 0.0f, -Projector.H - Projector.Dist);
    //
    //SetLinePos(11, 0, 0.0f, -Projector.H);
    //SetLinePos(11, 1, 0.0f, -Projector.H - Projector.Dist);
    //
    //SetLinePos(12, 0, 0.0f, -Projector.H - Projector.Dist);
    //SetLinePos(12, 1, Space.W, ch);
    //
    //SetLinePos(13, 0, 0.0f, -Projector.H);
    //SetLinePos(13, 1, Pyramid.W, -Projector.H - Projector.Dist - Pyramid.H);
    //
    //SetLinePos(14, 0, Pyramid.W, -Projector.H - Projector.Dist - Pyramid.H);
    //SetLinePos(14, 1, Space.W, cl);

    // Drawing the space.
    // Bottom of the space cube.
    // #1
    Lines[0].SetLine(START, -Space.W, 0.0f, 0.0f);
    Lines[0].SetLine(END, Space.W, 0.0f, 0.0f);
    // #2
    Lines[1].SetLine(START, Space.W, 0.0f, 0.0f);
    Lines[1].SetLine(END, Space.W, 0.0f, Space.D);
    //// #3
    Lines[2].SetLine(START, Space.W, 0.0f, Space.D);
    Lines[2].SetLine(END, -Space.W, 0.0f, Space.D);
    //// #4
    Lines[3].SetLine(START, -Space.W, 0.0f, Space.D);
    Lines[3].SetLine(END, -Space.W, 0.0f, 0.0f);
    // The #1 side of the space cube.
    // #1 - Already has been drawn.
    // #2
    Lines[4].SetLine(START, -Space.W, 0.0f, 0.0f);
    Lines[4].SetLine(END, -Space.W, Space.H, 0.0f);
    // #3
    Lines[5].SetLine(START, -Space.W, Space.H, 0.0f);
    Lines[5].SetLine(END, Space.W, Space.H, 0.0f);
    // #4
    Lines[6].SetLine(START, Space.W, Space.H, 0.0f);
    Lines[6].SetLine(END, Space.W, 0.0f, 0.0f);

    // The #2 side of the space cube.
    // #1 - already has been drawn.
    // #2 - already has been drawn.
    // #3
    Lines[7].SetLine(START, Space.W, Space.H, 0.0f);
    Lines[7].SetLine(END, Space.W, Space.H, Space.D);
    // #4
    Lines[8].SetLine(START, Space.W, Space.H, Space.D);
    Lines[8].SetLine(END, Space.W, 0.0f, Space.D);
    // The #3 side of the space cube.
    // #1 - already has been drawn.
    // #2 - already has been drawn.
    // #3
    Lines[9].SetLine(START, Space.W, Space.H, Space.D);
    Lines[9].SetLine(END, -Space.W, Space.H, Space.D);
    // #4
    Lines[10].SetLine(START, -Space.W, Space.H, Space.D);
    Lines[10].SetLine(END, -Space.W, 0.0f, Space.D);
    // The #4 side of the space cube.
    // #1 - already has been drawn.
    // #2 - already has been drawn.
    // #3 - already has been drawn.
    // #4
    Lines[11].SetLine(START, -Space.W, Space.H, Space.D);
    Lines[11].SetLine(END, -Space.W, Space.H, 0.0f);
    // Projector.
    // The top of the projector cube.
    // #1
    Lines[12].SetLine(START, Space.TopMiddle + new Vector3(Projector.W, 0.0f, Projector.D));
    Lines[12].SetLine(END, Space.TopMiddle + new Vector3(Projector.W, 0.0f, -Projector.D));
    // #2
    Lines[13].SetLine(START, Space.TopMiddle + new Vector3(Projector.W, 0.0f, -Projector.D));
    Lines[13].SetLine(END, Space.TopMiddle + new Vector3(-Projector.W, 0.0f, -Projector.D));
    // #3
    Lines[14].SetLine(START, Space.TopMiddle + new Vector3(Projector.W, 0.0f, Projector.D));
    Lines[14].SetLine(END, Space.TopMiddle + new Vector3(-Projector.W, 0.0f, Projector.D));
    // #4
    Lines[15].SetLine(START, Space.TopMiddle + new Vector3(-Projector.W, 0.0f, Projector.D));
    Lines[15].SetLine(END, Space.TopMiddle + new Vector3(-Projector.W, 0.0f, -Projector.D));
    // The #1 side of the projector cube.
    // #1
    Lines[16].SetLine(START, Space.TopMiddle + new Vector3(Projector.W, 0.0f, Projector.D));
    Lines[16].SetLine(END, Space.TopMiddle + new Vector3(Projector.W, -Projector.H, Projector.D));
    // #2
    Lines[17].SetLine(START, Space.TopMiddle + new Vector3(Projector.W, -Projector.H, Projector.D));
    Lines[17].SetLine(END, Space.TopMiddle + new Vector3(Projector.W, -Projector.H, -Projector.D));
    // #3
    Lines[18].SetLine(START, Space.TopMiddle + new Vector3(Projector.W, 0.0f, -Projector.D));
    Lines[18].SetLine(END, Space.TopMiddle + new Vector3(Projector.W, -Projector.H, -Projector.D));
    // #4 - already has been drawn
    // The #2 side of the projector cube.
    // #1
    Lines[19].SetLine(START, Space.TopMiddle + new Vector3(Projector.W, -Projector.H, -Projector.D));
    Lines[19].SetLine(END, Space.TopMiddle + new Vector3(-Projector.W, -Projector.H, -Projector.D));
    // #2
    Lines[20].SetLine(START, Space.TopMiddle + new Vector3(-Projector.W, 0.0f, -Projector.D));
    Lines[20].SetLine(END, Space.TopMiddle + new Vector3(-Projector.W, -Projector.H, -Projector.D));
    // #3
    // #4 - already has been drawn.
    // The #3 side of the projector cube.
    // #1
    Lines[21].SetLine(START, Space.TopMiddle + new Vector3(Projector.W, -Projector.H, Projector.D));
    Lines[21].SetLine(END, Space.TopMiddle + new Vector3(-Projector.W, -Projector.H, Projector.D));
    // #2
    Lines[22].SetLine(START, Space.TopMiddle + new Vector3(-Projector.W, 0.0f, Projector.D));
    Lines[22].SetLine(END, Space.TopMiddle + new Vector3(-Projector.W, -Projector.H, Projector.D));
    // #3
    Lines[23].SetLine(START, Space.TopMiddle + new Vector3(-Projector.W, -Projector.H, Projector.D));
    Lines[23].SetLine(END, Space.TopMiddle + new Vector3(-Projector.W, -Projector.H, -Projector.D));
    // Pyramid.
    // Distanced Line
    Lines[24].SetLineColor(DanbiLine.eColor.RED);
    Lines[24].SetLine(START, Projector.BottomMiddle);
    Lines[24].SetLine(END, Pyramid.Apex);
    //
    // The bottom of the pyramid.
    // #1
    Lines[25].SetLine(START, Pyramid.Origin + new Vector3(Pyramid.W, 0.0f, Pyramid.D));
    Lines[25].SetLine(END, Pyramid.Origin + new Vector3(Pyramid.W, 0.0f, -Pyramid.D));
    // #2
    Lines[26].SetLine(START, Pyramid.Origin + new Vector3(Pyramid.W, 0.0f, Pyramid.D));
    Lines[26].SetLine(END, Pyramid.Origin + new Vector3(-Pyramid.W, 0.0f, Pyramid.D));
    // #3
    Lines[27].SetLine(START, Pyramid.Origin + new Vector3(Pyramid.W, 0.0f, -Pyramid.D));
    Lines[27].SetLine(END, Pyramid.Origin + new Vector3(-Pyramid.W, 0.0f, -Pyramid.D));
    // #4
    Lines[28].SetLine(START, Pyramid.Origin + new Vector3(-Pyramid.W, 0.0f, Pyramid.D));
    Lines[28].SetLine(END, Pyramid.Origin + new Vector3(-Pyramid.W, 0.0f, -Pyramid.D));
    // The #1 side of the pyramid.
    Pyramid.Bottom1 = Pyramid.Origin + new Vector3(-Pyramid.W, 0.0f, -Pyramid.D);
    Lines[29].SetLine(START, Pyramid.Apex);
    Lines[29].SetLine(END, Pyramid.Bottom1);
    // The #2 side of the pyramid.
    Pyramid.Bottom2 = Pyramid.Origin + new Vector3(Pyramid.W, 0.0f, -Pyramid.D);
    Lines[30].SetLine(START, Pyramid.Apex);
    Lines[30].SetLine(END, Pyramid.Bottom2);
    // The #3 side of the pyramid.
    Pyramid.Bottom3 = Pyramid.Origin + new Vector3(Pyramid.W, 0.0f, Pyramid.D);
    Lines[31].SetLine(START, Pyramid.Apex);
    Lines[31].SetLine(END, Pyramid.Bottom3);
    // The #4 side of the pyramid.
    Pyramid.Bottom4 = Pyramid.Origin + new Vector3(-Pyramid.W, 0.0f, Pyramid.D);
    Lines[32].SetLine(START, Pyramid.Apex);
    Lines[32].SetLine(END, Pyramid.Bottom4);

    // Actual rays
    // #1
    // High-part
    // Lines[24].line.startColor = Color.red;
    Lines[33].SetLineColor(DanbiLine.eColor.RED);
    Lines[33].SetLine(START, Pyramid.Apex);
    Lines[33].SetLine(END, Space.W, ch, Space.D);
    // Low-part
    Lines[34].SetLineColor(DanbiLine.eColor.RED);
    Lines[34].SetLine(START, Pyramid.Bottom3);
    Lines[34].SetLine(END, Space.W, cl, Space.D);
    // #2
    // High-part
    Lines[35].SetLineColor(DanbiLine.eColor.BLUE);
    Lines[35].SetLine(START, Pyramid.Apex);
    Lines[35].SetLine(END, Space.W, ch, 0.0f);
    // Low-part
    Lines[36].SetLineColor(DanbiLine.eColor.BLUE);
    Lines[36].SetLine(START, Pyramid.Bottom2);
    Lines[36].SetLine(END, Space.W, cl, 0.0f);
    // #3
    // High-part
    Lines[37].SetLineColor(DanbiLine.eColor.GREEN);
    Lines[37].SetLine(START, Pyramid.Apex);
    Lines[37].SetLine(END, -Space.W, ch, 0.0f);
    // Low-part
    Lines[38].SetLineColor(DanbiLine.eColor.GREEN);
    Lines[38].SetLine(START, Pyramid.Bottom1);
    Lines[38].SetLine(END, -Space.W, cl, 0.0f);
    // #4
    // High-part
    Lines[39].SetLineColor(DanbiLine.eColor.YELLOW);
    Lines[39].SetLine(START, Pyramid.Apex);
    Lines[39].SetLine(END, -Space.W, ch, Space.W);
    // Low-part
    Lines[40].SetLineColor(DanbiLine.eColor.YELLOW);
    Lines[40].SetLine(START, Pyramid.Bottom4);
    Lines[40].SetLine(END, -Space.W, cl, Space.W);

    // auxiliary upper-horizontal lines for target.
    Lines[41].SetLineColor(DanbiLine.eColor.PURPLE);
    Lines[41].SetLine(START, Space.W, ch, Space.D);
    Lines[41].SetLine(END, -Space.W, ch, Space.D);

    Lines[42].SetLineColor(DanbiLine.eColor.PURPLE);
    Lines[42].SetLine(START, Space.W, ch, 0.0f);
    Lines[42].SetLine(END, -Space.W, ch, 0.0f);

    Lines[43].SetLineColor(DanbiLine.eColor.PURPLE);
    Lines[43].SetLine(START, -Space.W, ch, Space.W);
    Lines[43].SetLine(END, -Space.W, ch, 0.0f);

    Lines[44].SetLineColor(DanbiLine.eColor.PURPLE);
    Lines[44].SetLine(START, Space.W, ch, 0.0f);
    Lines[44].SetLine(END, Space.W, ch, Space.W);

    // auxiliary downer-horizontal lines for target.
    Lines[45].SetLineColor(DanbiLine.eColor.PURPLE);
    Lines[45].SetLine(START, Space.W, cl, Space.D);
    Lines[45].SetLine(END, -Space.W, cl, Space.D);

    Lines[46].SetLineColor(DanbiLine.eColor.PURPLE);
    Lines[46].SetLine(START, Space.W, cl, 0.0f);
    Lines[46].SetLine(END, -Space.W, cl, 0.0f);

    Lines[47].SetLineColor(DanbiLine.eColor.PURPLE);
    Lines[47].SetLine(START, -Space.W, cl, Space.W);
    Lines[47].SetLine(END, -Space.W, cl, 0.0f);

    Lines[48].SetLineColor(DanbiLine.eColor.PURPLE);
    Lines[48].SetLine(START, Space.W, cl, 0.0f);
    Lines[48].SetLine(END, Space.W, cl, Space.W);

    // auxiliary vertical lines for target.
    Lines[49].SetLineColor(DanbiLine.eColor.RED);
    Lines[49].SetLine(START, Space.W, 0.0f, Space.D);
    Lines[49].SetLine(END, Space.W, -Space.H * 2.0f, Space.D);

    Lines[50].SetLineColor(DanbiLine.eColor.YELLOW);
    Lines[50].SetLine(START, -Space.W, 0.0f, Space.D);
    Lines[50].SetLine(END, -Space.W, -Space.H * 2.0f, Space.D);

    Lines[51].SetLineColor(DanbiLine.eColor.BLUE);
    Lines[51].SetLine(START, Space.W, 0.0f, 0.0f);
    Lines[51].SetLine(END, Space.W, -Space.H * 2.0f, 0.0f);

    Lines[52].SetLineColor(DanbiLine.eColor.GREEN);
    Lines[52].SetLine(START, -Space.W, 0.0f, 0.0f);
    Lines[52].SetLine(END, -Space.W, -Space.H * 2.0f, 0.0f);

    // auxiliary vertical lines 2 for target.
    Lines[53].SetLineColor(DanbiLine.eColor.RED);
    Lines[53].SetLine(START, Space.W, 0.0f, Space.D);
    Lines[53].SetLine(END, Space.W, -Space.H * 2.0f, Space.D);

    Lines[54].SetLineColor(DanbiLine.eColor.YELLOW);
    Lines[54].SetLine(START, -Space.W, 0.0f, Space.D);
    Lines[54].SetLine(END, -Space.W, -Space.H * 2.0f, Space.D);

    Lines[55].SetLineColor(DanbiLine.eColor.BLUE);
    Lines[55].SetLine(START, Space.W, 0.0f, 0.0f);
    Lines[55].SetLine(END, Space.W, -Space.H * 2.0f, 0.0f);

    Lines[56].SetLineColor(DanbiLine.eColor.GREEN);
    Lines[56].SetLine(START, -Space.W, 0.0f, 0.0f);
    Lines[56].SetLine(END, -Space.W, -Space.H * 2.0f, 0.0f);
  }
};
