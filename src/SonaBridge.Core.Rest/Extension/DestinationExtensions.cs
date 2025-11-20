using System.Diagnostics.CodeAnalysis;

using SonaBridge.Core.Rest.Models;

using SsBaseInfoDest = SonaBridge.Core.Rest.Internal.Models.Speech_synthesis_base_information_destination;

namespace SonaBridge.Core.Rest.Extension;

/// <summary>
/// <see cref="Destination"/> 関連の拡張メソッドを提供します。
/// </summary>
[SuppressMessage("Naming", "CA1708", Justification = "https://github.com/dotnet/sdk/issues/51716")]
public static class DestinationExtensions
{
	extension(Destination destination)
	{
		/// <summary>
		/// <see cref="Destination"/> を <see cref="SsBaseInfoDest"/> に変換します。
		/// </summary>
		/// <param name="internalDestination">変換された内部  <see cref="Destination"/> 値</param>
		/// <returns>変換に成功した場合は <see langword="true"/>、失敗した場合は <see langword="false"/> を返します。</returns>
		/// <seealso cref="ForceToInternal(Destination)"/>
		[SuppressMessage("Usage", "SMA0020:Unchecked Cast to Enum Type", Justification = "not casting")]
		public bool TryToInternal(
			out SsBaseInfoDest? internalDestination
		)
		{
			internalDestination = destination switch
			{
				Destination.AudioDevice => SsBaseInfoDest.Audio_device,
				Destination.File => SsBaseInfoDest.File,
				_ => null,
			};
			return internalDestination is not null;
		}

		/// <summary>
		/// <see cref="Destination"/> を <see cref="SsBaseInfoDest"/> に強制変換します。
		/// 既知の値以外の場合は <see cref="SsBaseInfoDest.Audio_device"/> を返します。
		/// </summary>
		/// <returns>変換された内部 <see cref="Destination"/> 値</returns>
		/// <seealso cref="TryToInternal(Destination)"/>
		public SsBaseInfoDest ForceToInternal() =>
			destination switch
			{
				Destination.AudioDevice => SsBaseInfoDest.Audio_device,
				Destination.File => SsBaseInfoDest.File,
				_ => SsBaseInfoDest.Audio_device,
			};
	}

	/// <summary>
	/// <see cref="SsBaseInfoDest"/> の拡張メソッドを提供します。
	/// </summary>
	/// <seealso cref="DestinationExtensions"/>
	extension(SsBaseInfoDest internalDestination)
	{
		/// <summary>
		/// <see cref="SsBaseInfoDest"/> を <see cref="Destination"/> に変換します。
		/// </summary>
		/// <returns>変換された <see cref="Destination"/> 値</returns>
		public Destination ToPublic() =>
			internalDestination switch
			{
				SsBaseInfoDest.Audio_device => Destination.AudioDevice,
				SsBaseInfoDest.File => Destination.File,
				_ => Destination.Unknown,
			};
	}
}
