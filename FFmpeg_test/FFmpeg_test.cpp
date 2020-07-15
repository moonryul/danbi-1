extern "C" {
#include <libavformat/avformat.h>
#include <libavcodec/avcodec.h>
#include <libavdevice/avdevice.h>
#include <libavfilter/avfilter.h>
#include <libavformat/avformat.h>
#include <libavutil/avutil.h>
#include <libswscale/swscale.h>
#include <libavutil/imgutils.h>
#include <libswresample/swresample.h>
}

#include <cstdio>
#include <Windows.h>
#include <utility>
#include <memory>

#define SAFE_DEL(p) \
do { delete p; p = nullptr; } while (p != nullptr);

#define SAFE_DEL_ARR(pArr) \
do { delete[] pArr; pArr = nullptr; } while(pArr != nullptr);

namespace {
  const char* fiesta_file_path = "c:\\Dev\\rsrc\\fiesta.mp4";
}

int main() {
#pragma region Video file open/close
  //AVFormatContext* fmt_ctx = nullptr;
  //int vidx = -1, aidx = -1;
  //const int ret = avformat_open_input(&fmt_ctx, fiesta_file_path, nullptr, nullptr);
  //if (ret != 0) {
  //  return -1;
  //}

  //avformat_find_stream_info(fmt_ctx, nullptr);

  //// 1. Iterate all the streams for seeking the streams.
  ///*for (unsigned i = 0; i < fmt_ctx->nb_streams; ++i) {
  //  if (fmt_ctx->streams[i]->codecpar->codec_type == AVMEDIA_TYPE_VIDEO) {
  //    vidx = i;
  //  }

  //  if (fmt_ctx->streams[i]->codecpar->codec_type == AVMEDIA_TYPE_AUDIO) {
  //    aidx = i;
  //  }
  //}*/

  //// 2. Another way to find the streams
  //vidx = av_find_best_stream (fmt_ctx, AVMEDIA_TYPE_VIDEO, -1, -1, nullptr, 0);
  //aidx = av_find_best_stream (fmt_ctx, AVMEDIA_TYPE_AUDIO, -1, vidx, nullptr, 0);

  //printf("video = %d번, audio = %d번\n\n\n", vidx, aidx);
  //av_dump_format(fmt_ctx, vidx, fiesta_file_path, 0);  
  //avformat_close_input(&fmt_ctx);
  //fmt_ctx = nullptr;
  //return 0;
#pragma endregion Video file open/close

#pragma region Inquery the info of the stream
  /*AVFormatContext* fmt_ctx = nullptr;

  const int ret = avformat_open_input(&fmt_ctx, ::fiesta_file_path, nullptr, nullptr);
  if (ret != 0) {
    return -1;
  }

  avformat_find_stream_info(fmt_ctx, nullptr);

  const int video_idx = av_find_best_stream(fmt_ctx, AVMEDIA_TYPE_VIDEO, -1, -1, nullptr, 0);
  const int audio_idx = av_find_best_stream(fmt_ctx, AVMEDIA_TYPE_AUDIO, -1, video_idx, nullptr, 0);

  auto * video_stream = fmt_ctx->streams[video_idx];
  printf("프레임 개수 = %l64\n", video_stream->nb_frames);
  printf("프레임 레이트 = %d / %d\n", video_stream->avg_frame_rate.num, video_stream->avg_frame_rate.den);
  printf("타임 베이스 = %d / %d\n", video_stream->time_base.num, video_stream->time_base.den);

  auto * video_parameters = video_stream->codecpar;
  printf("비디오 해상도 = %d x %d\n", video_parameters->width, video_parameters->height);
  printf("색상 포맷 = %d\n", video_parameters->format);
  printf("코덱 = %d\n", video_parameters->codec_id);

  printf("\n------------------------------------------\n\n");

  auto * audio_stream = fmt_ctx->streams[audio_idx];
  printf("프레임 수 = %l64\n", audio_stream->nb_frames);
  printf("타임 베이스 = %d / %d\n", audio_stream->time_base.num, audio_stream->time_base.den);

  auto * audio_parameters = audio_stream->codecpar;
  printf("사운드 포맷 = %d\n", audio_parameters->format);
  printf("코덱 = %d\n", audio_parameters->codec_id);
  printf("채널 = %d\n", audio_parameters->channels);
  printf("샘플 레이트 = %d\n", audio_parameters->sample_rate);

  avformat_close_input(&fmt_ctx);*/
#pragma endregion Inquery the info of the stream

#pragma region Codec operations
  AVFormatContext* fmt_ctx = nullptr;
  const int res = avformat_open_input(&fmt_ctx, fiesta_file_path, nullptr, nullptr);
  if (res != 0) {
    return -1;
  }
  avformat_find_stream_info(fmt_ctx, nullptr);

  const int vidx = av_find_best_stream(fmt_ctx, AVMEDIA_TYPE_VIDEO, -1, -1, nullptr, 0);
  const int aidx = av_find_best_stream(fmt_ctx, AVMEDIA_TYPE_AUDIO, -1, vidx, nullptr, 0);

  // open the video codec.
  auto* vStream = fmt_ctx->streams[vidx];
  auto* vParam = vStream->codecpar;
  auto* vCodec = avcodec_find_decoder(vParam->codec_id);
  auto* vCtx = avcodec_alloc_context3(vCodec);
  avcodec_parameters_to_context(vCtx, vParam);
  avcodec_open2(vCtx, vCodec, nullptr);

  // open the audio codec.
  auto* aStream = fmt_ctx->streams[aidx];
  auto* aParam = aStream->codecpar;
  auto* aCodec = avcodec_find_decoder(aParam->codec_id);
  auto* aCtx = avcodec_alloc_context3(aCodec);
  avcodec_parameters_to_context(aCtx, aParam);
  avcodec_open2(aCtx, aCodec, nullptr);

  // Inquery the codec info.
  printf("비디오 코덱 : %d, %s(%s)\n", vCodec->id, vCodec->name, vCodec->long_name);
  printf("수용 능력 : %x\n", vCodec->capabilities);
  printf("오디오 코덱 : %d, %s(%s)\n", aCodec->id, aCodec->name, aCodec->long_name);
  printf("수용 능력 : %x\n", aCodec->capabilities);

  avcodec_free_context(&vCtx);
  avcodec_free_context(&aCtx);
  avformat_close_input(&fmt_ctx);

  /*SAFE_DEL(vStream);
  SAFE_DEL(vParam);
  SAFE_DEL(vCodec);
  SAFE_DEL(aStream);
  SAFE_DEL(aParam);
  SAFE_DEL(aCodec);*/
  return 0;
#pragma endregion Codec operations
}
