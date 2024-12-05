using MPF.ExecutionContexts.Aaru;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.ExecutionContexts.Test
{
    public class AaruTests
    {
        #region Converters.Extension

        [Theory]
        [InlineData(null, ".aaruf")]
        [InlineData(MediaType.CDROM, ".aaruf")]
        [InlineData(MediaType.GDROM, ".aaruf")]
        [InlineData(MediaType.DVD, ".aaruf")]
        [InlineData(MediaType.HDDVD, ".aaruf")]
        [InlineData(MediaType.BluRay, ".aaruf")]
        [InlineData(MediaType.FloppyDisk, ".aaruf")]
        [InlineData(MediaType.HardDisk, ".aaruf")]
        [InlineData(MediaType.ApertureCard, ".aaruf")]
        public void ExtensionTest(MediaType? type, string expected)
        {
            string actual = Converters.Extension(type);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Archive Family

        [Theory]
        [InlineData("arc info filename.bin")]
        [InlineData("arc info \"filename.bin\"")]
        [InlineData("archive info filename.bin")]
        [InlineData("archive info \"filename.bin\"")]
        public void ArchiveInfoTest(string parameters)
        {
            string? expected = "archive info \"filename.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Database Family

        [Theory]
        [InlineData("db stats")]
        [InlineData("database stats")]
        public void DatabaseStatsTest(string parameters)
        {
            string? expected = "database stats";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("db update --clear --clear-all")]
        [InlineData("db update --clear true --clear-all true")]
        [InlineData("database update --clear --clear-all")]
        [InlineData("database update --clear true --clear-all true")]
        public void DatabaseUpdateTest(string parameters)
        {
            string? expected = "database update --clear True --clear-all True";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Device Family

        [Theory]
        [InlineData("dev info -w prefix filename.bin")]
        [InlineData("dev info --output-prefix prefix filename.bin")]
        [InlineData("device info -w prefix filename.bin")]
        [InlineData("device info --output-prefix prefix filename.bin")]
        public void DeviceInfoTest(string parameters)
        {
            string? expected = "device info --output-prefix \"prefix\" filename.bin";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("dev list localhost")]
        [InlineData("device list localhost")]
        public void DeviceListTest(string parameters)
        {
            string? expected = "device list \"localhost\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("dev report -t filename.bin")]
        [InlineData("dev report -t true filename.bin")]
        [InlineData("dev report --trap-disc filename.bin")]
        [InlineData("dev report --trap-disc true filename.bin")]
        [InlineData("device report -t filename.bin")]
        [InlineData("device report -t true filename.bin")]
        [InlineData("device report --trap-disc filename.bin")]
        [InlineData("device report --trap-disc true filename.bin")]
        public void DeviceReportTest(string parameters)
        {
            string? expected = "device report --trap-disc True filename.bin";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Filesystem Family

        [Theory]
        [InlineData("fi extract -e enc -x -n ns -O opts input output")]
        [InlineData("fi extract -e enc -x true -n ns -O opts input output")]
        [InlineData("fi extract --encoding enc --xattrs --namespace ns --options opts input output")]
        [InlineData("fi extract --encoding enc --xattrs true --namespace ns --options opts input output")]
        [InlineData("fs extract -e enc -x -n ns -O opts input output")]
        [InlineData("fs extract -e enc -x true -n ns -O opts input output")]
        [InlineData("fs extract --encoding enc --xattrs --namespace ns --options opts input output")]
        [InlineData("fs extract --encoding enc --xattrs true --namespace ns --options opts input output")]
        [InlineData("filesystem extract -e enc -x -n ns -O opts input output")]
        [InlineData("filesystem extract -e enc -x true -n ns -O opts input output")]
        [InlineData("filesystem extract --encoding enc --xattrs --namespace ns --options opts input output")]
        [InlineData("filesystem extract --encoding enc --xattrs true --namespace ns --options opts input output")]
        public void FilesystemExtractTest(string parameters)
        {
            string? expected = "filesystem extract --xattrs True --encoding \"enc\" --namespace \"ns\" --options \"opts\" \"input\" \"output\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("fi info -e enc -x -n ns -O opts input")]
        [InlineData("fi info -e enc -x true -n ns -O opts input")]
        [InlineData("fi info --encoding enc --xattrs --namespace ns --options opts input")]
        [InlineData("fi info --encoding enc --xattrs true --namespace ns --options opts input")]
        [InlineData("fs info -e enc -x -n ns -O opts input")]
        [InlineData("fs info -e enc -x true -n ns -O opts input")]
        [InlineData("fs info --encoding enc --xattrs --namespace ns --options opts input")]
        [InlineData("fs info --encoding enc --xattrs true --namespace ns --options opts input")]
        [InlineData("filesystem info -e enc -x -n ns -O opts input")]
        [InlineData("filesystem info -e enc -x true -n ns -O opts input")]
        [InlineData("filesystem info --encoding enc --xattrs --namespace ns --options opts input")]
        [InlineData("filesystem info --encoding enc --xattrs true --namespace ns --options opts input")]
        public void FilesystemInfoTest(string parameters)
        {
            string? expected = "filesystem info --xattrs True --encoding \"enc\" --namespace \"ns\" --options \"opts\" \"input\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("fi ls -e enc -f -l -p input")]
        [InlineData("fi list -e enc -f -l -p input")]
        [InlineData("fi ls -e enc -f true -l true -p true input")]
        [InlineData("fi list -e enc -f true -l true -p true input")]
        [InlineData("fi ls --encoding enc --filesystems --long-format --partitions input")]
        [InlineData("fi list --encoding enc --filesystems --long-format --partitions input")]
        [InlineData("fi ls --encoding enc --filesystems true --long-format true --partitions true input")]
        [InlineData("fi list --encoding enc --filesystems true --long-format true --partitions true input")]
        [InlineData("fs ls -e enc -f -l -p input")]
        [InlineData("fs list -e enc -f -l -p input")]
        [InlineData("fs ls -e enc -f true -l true -p true input")]
        [InlineData("fs list -e enc -f true -l true -p true input")]
        [InlineData("fs ls --encoding enc --filesystems --long-format --partitions input")]
        [InlineData("fs list --encoding enc --filesystems --long-format --partitions input")]
        [InlineData("fs ls --encoding enc --filesystems true --long-format true --partitions true input")]
        [InlineData("fs list --encoding enc --filesystems true --long-format true --partitions true input")]
        [InlineData("filesystem ls -e enc -f -l -p input")]
        [InlineData("filesystem list -e enc -f -l -p input")]
        [InlineData("filesystem ls -e enc -f true -l true -p true input")]
        [InlineData("filesystem list -e enc -f true -l true -p true input")]
        [InlineData("filesystem ls --encoding enc --filesystems --long-format --partitions input")]
        [InlineData("filesystem list --encoding enc --filesystems --long-format --partitions input")]
        [InlineData("filesystem ls --encoding enc --filesystems true --long-format true --partitions true input")]
        [InlineData("filesystem list --encoding enc --filesystems true --long-format true --partitions true input")]
        public void FilesystemListTest(string parameters)
        {
            string? expected = "filesystem list --filesystems True --long-format True --partitions True --encoding \"enc\" \"input\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("fi options")]
        [InlineData("fs options")]
        [InlineData("filesystem options")]
        public void FilesystemOptionsTest(string parameters)
        {
            string? expected = "filesystem options";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Image Family

        [Theory]
        [InlineData("i chk -a --crc16 -c --crc64 --fletcher16 --fletcher32 -m -t -s --sha256 --sha384 --sha512 -f -w filename.bin")]
        [InlineData("i chk -a true --crc16 true -c true --crc64 true --fletcher16 true --fletcher32 true -m true -t true -s true --sha256 true --sha384 true --sha512 true -f true -w true filename.bin")]
        [InlineData("i chk --adler32 --crc16 --crc32 --crc64 --fletcher16 --fletcher32 --md5 --separated-tracks --sha1 --sha256 --sha384 --sha512 --spamsum --whole-disc filename.bin")]
        [InlineData("i chk --adler32 true --crc16 true --crc32 true --crc64 true --fletcher16 true --fletcher32 true --md5 true --separated-tracks true --sha1 true --sha256 true --sha384 true --sha512 true --spamsum true --whole-disc true filename.bin")]
        [InlineData("i checksum -a --crc16 -c --crc64 --fletcher16 --fletcher32 -m -t -s --sha256 --sha384 --sha512 -f -w filename.bin")]
        [InlineData("i checksum -a true --crc16 true -c true --crc64 true --fletcher16 true --fletcher32 true -m true -t true -s true --sha256 true --sha384 true --sha512 true -f true -w true filename.bin")]
        [InlineData("i checksum --adler32 --crc16 --crc32 --crc64 --fletcher16 --fletcher32 --md5 --separated-tracks --sha1 --sha256 --sha384 --sha512 --spamsum --whole-disc filename.bin")]
        [InlineData("i checksum --adler32 true --crc16 true --crc32 true --crc64 true --fletcher16 true --fletcher32 true --md5 true --separated-tracks true --sha1 true --sha256 true --sha384 true --sha512 true --spamsum true --whole-disc true filename.bin")]
        [InlineData("image chk -a --crc16 -c --crc64 --fletcher16 --fletcher32 -m -t -s --sha256 --sha384 --sha512 -f -w filename.bin")]
        [InlineData("image chk -a true --crc16 true -c true --crc64 true --fletcher16 true --fletcher32 true -m true -t true -s true --sha256 true --sha384 true --sha512 true -f true -w true filename.bin")]
        [InlineData("image chk --adler32 --crc16 --crc32 --crc64 --fletcher16 --fletcher32 --md5 --separated-tracks --sha1 --sha256 --sha384 --sha512 --spamsum --whole-disc filename.bin")]
        [InlineData("image chk --adler32 true --crc16 true --crc32 true --crc64 true --fletcher16 true --fletcher32 true --md5 true --separated-tracks true --sha1 true --sha256 true --sha384 true --sha512 true --spamsum true --whole-disc true filename.bin")]
        [InlineData("image checksum -a --crc16 -c --crc64 --fletcher16 --fletcher32 -m -t -s --sha256 --sha384 --sha512 -f -w filename.bin")]
        [InlineData("image checksum -a true --crc16 true -c true --crc64 true --fletcher16 true --fletcher32 true -m true -t true -s true --sha256 true --sha384 true --sha512 true -f true -w true filename.bin")]
        [InlineData("image checksum --adler32 --crc16 --crc32 --crc64 --fletcher16 --fletcher32 --md5 --separated-tracks --sha1 --sha256 --sha384 --sha512 --spamsum --whole-disc filename.bin")]
        [InlineData("image checksum --adler32 true --crc16 true --crc32 true --crc64 true --fletcher16 true --fletcher32 true --md5 true --separated-tracks true --sha1 true --sha256 true --sha384 true --sha512 true --spamsum true --whole-disc true filename.bin")]
        public void ImageChecksumTest(string parameters)
        {
            string? expected = "image checksum --adler32 True --crc16 True --crc32 True --crc64 True --fletcher16 True --fletcher32 True --md5 True --separated-tracks True --sha1 True --sha256 True --sha384 True --sha512 True --spamsum True --whole-disc True \"filename.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("i cmp input1.bin input2.bin")]
        [InlineData("i compare input1.bin input2.bin")]
        [InlineData("image cmp input1.bin input2.bin")]
        [InlineData("image compare input1.bin input2.bin")]
        public void ImageCompareTest(string parameters)
        {
            string? expected = "image compare \"input1.bin\" \"input2.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("i convert --comments co -c 0 --creator cr --drive-manufacturer dm --drive-model dm --drive-revision dr --drive-serial ds --fix-subchannel --fix-subchannel-crc --fix-subchannel-position -f -p fmt --generate-subchannels -g geo --media-barcode mb --media-lastsequence 0 --media-manufacturer mm --media-model mm --media-partnumber mpn --media-sequence 0 --media-serial ms --media-title mt -O opt -r resume -x cicm input1.bin input2.bin")]
        [InlineData("i convert --comments co -c 0 --creator cr --drive-manufacturer dm --drive-model dm --drive-revision dr --drive-serial ds --fix-subchannel true --fix-subchannel-crc true --fix-subchannel-position true -f true -p fmt --generate-subchannels true -g geo --media-barcode mb --media-lastsequence 0 --media-manufacturer mm --media-model mm --media-partnumber mpn --media-sequence 0 --media-serial ms --media-title mt -O opt -r resume -x cicm input1.bin input2.bin")]
        [InlineData("i convert --comments co --count 0 --creator cr --drive-manufacturer dm --drive-model dm --drive-revision dr --drive-serial ds --fix-subchannel --fix-subchannel-crc --fix-subchannel-position --force --format fmt --generate-subchannels --geometry geo --media-barcode mb --media-lastsequence 0 --media-manufacturer mm --media-model mm --media-partnumber mpn --media-sequence 0 --media-serial ms --media-title mt --options opt --resume-file resume --cicm-xml cicm input1.bin input2.bin")]
        [InlineData("i convert --comments co --count 0 --creator cr --drive-manufacturer dm --drive-model dm --drive-revision dr --drive-serial ds --fix-subchannel true --fix-subchannel-crc true --fix-subchannel-position true --force true --format fmt --generate-subchannels true --geometry geo --media-barcode mb --media-lastsequence 0 --media-manufacturer mm --media-model mm --media-partnumber mpn --media-sequence 0 --media-serial ms --media-title mt --options opt --resume-file resume --cicm-xml cicm input1.bin input2.bin")]
        [InlineData("image convert --comments co -c 0 --creator cr --drive-manufacturer dm --drive-model dm --drive-revision dr --drive-serial ds --fix-subchannel --fix-subchannel-crc --fix-subchannel-position -f -p fmt --generate-subchannels -g geo --media-barcode mb --media-lastsequence 0 --media-manufacturer mm --media-model mm --media-partnumber mpn --media-sequence 0 --media-serial ms --media-title mt -O opt -r resume -x cicm input1.bin input2.bin")]
        [InlineData("image convert --comments co -c 0 --creator cr --drive-manufacturer dm --drive-model dm --drive-revision dr --drive-serial ds --fix-subchannel true --fix-subchannel-crc true --fix-subchannel-position true -f true -p fmt --generate-subchannels true -g geo --media-barcode mb --media-lastsequence 0 --media-manufacturer mm --media-model mm --media-partnumber mpn --media-sequence 0 --media-serial ms --media-title mt -O opt -r resume -x cicm input1.bin input2.bin")]
        [InlineData("image convert --comments co --count 0 --creator cr --drive-manufacturer dm --drive-model dm --drive-revision dr --drive-serial ds --fix-subchannel --fix-subchannel-crc --fix-subchannel-position --force --format fmt --generate-subchannels --geometry geo --media-barcode mb --media-lastsequence 0 --media-manufacturer mm --media-model mm --media-partnumber mpn --media-sequence 0 --media-serial ms --media-title mt --options opt --resume-file resume --cicm-xml cicm input1.bin input2.bin")]
        [InlineData("image convert --comments co --count 0 --creator cr --drive-manufacturer dm --drive-model dm --drive-revision dr --drive-serial ds --fix-subchannel true --fix-subchannel-crc true --fix-subchannel-position true --force true --format fmt --generate-subchannels true --geometry geo --media-barcode mb --media-lastsequence 0 --media-manufacturer mm --media-model mm --media-partnumber mpn --media-sequence 0 --media-serial ms --media-title mt --options opt --resume-file resume --cicm-xml cicm input1.bin input2.bin")]
        public void ImageConvertTest(string parameters)
        {
            string? expected = "image convert --fix-subchannel True --fix-subchannel-crc True --fix-subchannel-position True --force True --generate-subchannels True --count 0 --media-lastsequence 0 --media-sequence 0 --comments \"co\" --creator \"cr\" --drive-manufacturer \"dm\" --drive-model \"dm\" --drive-revision \"dr\" --drive-serial \"ds\" --format \"fmt\" --geometry \"geo\" --media-barcode \"mb\" --media-manufacturer \"mm\" --media-model \"mm\" --media-partnumber \"mpn\" --media-serial \"ms\" --media-title \"mt\" --options \"opt\" --resume-file \"resume\" --cicm-xml \"cicm\" \"input1.bin\" \"input2.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("i create-sidecar -b 0 -e enc -t filename.bin")]
        [InlineData("i create-sidecar -b 0 -e enc -t true filename.bin")]
        [InlineData("i create-sidecar --block-size 0 --encoding enc --tape filename.bin")]
        [InlineData("i create-sidecar --block-size 0 --encoding enc --tape true filename.bin")]
        [InlineData("image create-sidecar -b 0 -e enc -t filename.bin")]
        [InlineData("image create-sidecar -b 0 -e enc -t true filename.bin")]
        [InlineData("image create-sidecar --block-size 0 --encoding enc --tape filename.bin")]
        [InlineData("image create-sidecar --block-size 0 --encoding enc --tape true filename.bin")]
        public void ImageCreateSidecarTest(string parameters)
        {
            string? expected = "image create-sidecar --tape True --block-size 0 --encoding \"enc\" \"filename.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("i decode -f -l all -p -s 0 filename.bin")]
        [InlineData("i decode -f true -l all -p true -s 0 filename.bin")]
        [InlineData("i decode --disk-tags --length all --sector-tags --start 0 filename.bin")]
        [InlineData("i decode --disk-tags true --length all --sector-tags true --start 0 filename.bin")]
        [InlineData("image decode -f -l all -p -s 0 filename.bin")]
        [InlineData("image decode -f true -l all -p true -s 0 filename.bin")]
        [InlineData("image decode --disk-tags --length all --sector-tags --start 0 filename.bin")]
        [InlineData("image decode --disk-tags true --length all --sector-tags true --start 0 filename.bin")]
        public void ImageDecodeTest(string parameters)
        {
            string? expected = "image decode --disk-tags True --sector-tags True --length all --start 0 \"filename.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("i entropy -p -t -w filename.bin")]
        [InlineData("i entropy -p true -t true -w true filename.bin")]
        [InlineData("i entropy --duplicated-sectors --separated-tracks --whole-disc filename.bin")]
        [InlineData("i entropy --duplicated-sectors true --separated-tracks true --whole-disc true filename.bin")]
        [InlineData("image entropy -p -t -w filename.bin")]
        [InlineData("image entropy -p true -t true -w true filename.bin")]
        [InlineData("image entropy --duplicated-sectors --separated-tracks --whole-disc filename.bin")]
        [InlineData("image entropy --duplicated-sectors true --separated-tracks true --whole-disc true filename.bin")]
        public void ImageEntropyTest(string parameters)
        {
            string? expected = "image entropy --duplicated-sectors True --separated-tracks True --whole-disc True \"filename.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("i info filename.bin")]
        [InlineData("image info filename.bin")]
        public void ImageInfoTest(string parameters)
        {
            string? expected = "image info \"filename.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("i options")]
        [InlineData("image options")]
        public void ImageOptionsTest(string parameters)
        {
            string? expected = "image options";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("i print -l 0 -r -s 0 -w 0 filename.bin")]
        [InlineData("i print -l 0 -r true -s 0 -w 0 filename.bin")]
        [InlineData("i print --length 0 --long-sectors --start 0 --width 0 filename.bin")]
        [InlineData("i print --length 0 --long-sectors true --start 0 --width 0 filename.bin")]
        [InlineData("image print -l 0 -r -s 0 -w 0 filename.bin")]
        [InlineData("image print -l 0 -r true -s 0 -w 0 filename.bin")]
        [InlineData("image print --length 0 --long-sectors --start 0 --width 0 filename.bin")]
        [InlineData("image print --length 0 --long-sectors true --start 0 --width 0 filename.bin")]
        public void ImagePrintTest(string parameters)
        {
            string? expected = "image print --long-sectors True --width 0 --length 0 --start 0 \"filename.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("i verify -w -s filename.bin")]
        [InlineData("i verify -w true -s true filename.bin")]
        [InlineData("i verify --verify-disc --verify-sectors filename.bin")]
        [InlineData("i verify --verify-disc true --verify-sectors true filename.bin")]
        [InlineData("image verify -w -s filename.bin")]
        [InlineData("image verify -w true -s true filename.bin")]
        [InlineData("image verify --verify-disc --verify-sectors filename.bin")]
        [InlineData("image verify --verify-disc true --verify-sectors true filename.bin")]
        public void ImageVerifyTest(string parameters)
        {
            string? expected = "image verify --verify-disc True --verify-sectors True \"filename.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Media Family

        [Theory]
        [InlineData("m dump --eject -e enc --first-pregap --fix-offset --fix-subchannel --fix-subchannel-crc --fix-subchannel-position -f --generate-subchannels --max-blocks 0 --metadata -O opt --persistent --private -r -p 0 --retry-subchannel -k 0 --skip-cdiready-hole --speed 0 -s --store-encrypted --subchannel any --title-keys --trim --use-buffered-reads -x cicm input output.bin")]
        [InlineData("m dump --eject true -e enc --first-pregap true --fix-offset true --fix-subchannel true --fix-subchannel-crc true --fix-subchannel-position true -f true --generate-subchannels true --max-blocks 0 --metadata true -O opt --persistent true --private true -r true -p 0 --retry-subchannel true -k 0 --skip-cdiready-hole true --speed 0 -s true --store-encrypted true --subchannel any --title-keys true --trim true --use-buffered-reads true -x cicm input output.bin")]
        [InlineData("m dump --eject --encoding enc --first-pregap --fix-offset --fix-subchannel --fix-subchannel-crc --fix-subchannel-position --force --generate-subchannels --max-blocks 0 --metadata --options opt --persistent --private --resume --retry-passes 0 --retry-subchannel --skip 0 --skip-cdiready-hole --speed 0 --stop-on-error --store-encrypted --subchannel any --title-keys --trim --use-buffered-reads --cicm-xml cicm input output.bin")]
        [InlineData("m dump --eject true --encoding enc --first-pregap true --fix-offset true --fix-subchannel true --fix-subchannel-crc true --fix-subchannel-position true --force true --generate-subchannels true --max-blocks 0 --metadata true --options opt --persistent true --private true --resume true --retry-passes 0 --retry-subchannel true --skip 0 --skip-cdiready-hole true --speed 0 --stop-on-error true --store-encrypted true --subchannel any --title-keys true --trim true --use-buffered-reads true --cicm-xml cicm input output.bin")]
        [InlineData("media dump --eject -e enc --first-pregap --fix-offset --fix-subchannel --fix-subchannel-crc --fix-subchannel-position -f --generate-subchannels --max-blocks 0 --metadata -O opt --persistent --private -r -p 0 --retry-subchannel -k 0 --skip-cdiready-hole --speed 0 -s --store-encrypted --subchannel any --title-keys --trim --use-buffered-reads -x cicm input output.bin")]
        [InlineData("media dump --eject true -e enc --first-pregap true --fix-offset true --fix-subchannel true --fix-subchannel-crc true --fix-subchannel-position true -f true --generate-subchannels true --max-blocks 0 --metadata true -O opt --persistent true --private true -r true -p 0 --retry-subchannel true -k 0 --skip-cdiready-hole true --speed 0 -s true --store-encrypted true --subchannel any --title-keys true --trim true --use-buffered-reads true -x cicm input output.bin")]
        [InlineData("media dump --eject --encoding enc --first-pregap --fix-offset --fix-subchannel --fix-subchannel-crc --fix-subchannel-position --force --generate-subchannels --max-blocks 0 --metadata --options opt --persistent --private --resume --retry-passes 0 --retry-subchannel --skip 0 --skip-cdiready-hole --speed 0 --stop-on-error --store-encrypted --subchannel any --title-keys --trim --use-buffered-reads --cicm-xml cicm input output.bin")]
        [InlineData("media dump --eject true --encoding enc --first-pregap true --fix-offset true --fix-subchannel true --fix-subchannel-crc true --fix-subchannel-position true --force true --generate-subchannels true --max-blocks 0 --metadata true --options opt --persistent true --private true --resume true --retry-passes 0 --retry-subchannel true --skip 0 --skip-cdiready-hole true --speed 0 --stop-on-error true --store-encrypted true --subchannel any --title-keys true --trim true --use-buffered-reads true --cicm-xml cicm input output.bin")]
        public void MediaDumpTest(string parameters)
        {
            string? expected = "media dump --eject True --first-pregap True --fix-offset True --fix-subchannel True --fix-subchannel-crc True --fix-subchannel-position True --force True --generate-subchannels True --metadata True --persistent True --private True --resume True --retry-subchannel True --skip-cdiready-hole True --stop-on-error True --store-encrypted True --title-keys True --trim True --use-buffered-reads True --speed 0 --retry-passes 0 --max-blocks 0 --skip 0 --encoding \"enc\" --options \"opt\" --subchannel \"any\" --cicm-xml \"cicm\" input \"output.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("m info -w prefix input")]
        [InlineData("m info --output-prefix prefix input")]
        [InlineData("media info -w prefix input")]
        [InlineData("media info --output-prefix prefix input")]
        public void MediaInfoTest(string parameters)
        {
            string? expected = "media info --output-prefix \"prefix\" input";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("m scan -b ibg -m mhdd --use-buffered-reads input")]
        [InlineData("m scan -b ibg -m mhdd --use-buffered-reads true input")]
        [InlineData("m scan --ibg-log ibg --mhdd-log mhdd --use-buffered-reads input")]
        [InlineData("m scan --ibg-log ibg --mhdd-log mhdd --use-buffered-reads true input")]
        [InlineData("media scan -b ibg -m mhdd --use-buffered-reads input")]
        [InlineData("media scan -b ibg -m mhdd --use-buffered-reads true input")]
        [InlineData("media scan --ibg-log ibg --mhdd-log mhdd --use-buffered-reads input")]
        [InlineData("media scan --ibg-log ibg --mhdd-log mhdd --use-buffered-reads true input")]
        public void MediaScanTest(string parameters)
        {
            string? expected = "media scan --use-buffered-reads True --ibg-log \"ibg\" --mhdd-log \"mhdd\" input";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Standalone Commands

        [Theory]
        [InlineData("--debug --help --verbose --version formats")]
        [InlineData("--debug true --help true --verbose true --version true formats")]
        public void PreCommandFlagsTest(string parameters)
        {
            string? expected = "--debug True --help True --verbose True --version True formats";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("configure", "configure")]
        [InlineData("formats", "formats")]
        [InlineData("list-encodings", "list-encodings")]
        [InlineData("list-namespaces", "list-namespaces")]
        [InlineData("remote localhost", "remote \"localhost\"")]
        public void StandaloneCommandsTest(string parameters, string? expected)
        {
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}