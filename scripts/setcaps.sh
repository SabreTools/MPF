#!/bin/sh
# Lets redumper and DiscImageCreator from an MPF Linux release dump without sudo.
#
# Some drive commands that these two programs send, such as the Plextor
# reads, are only allowed for programs with the Linux capability
# CAP_SYS_RAWIO. This script gives that one capability to these two
# programs, so neither they nor MPF have to run as root.
#
# Unpacking MPF loses the capability, so run this again after every update.
#
# Usage: sudo sh setcaps.sh /path/to/MPF

if [ "$(id -u)" -ne 0 ] || [ -z "$1" ]; then
    echo "Usage: sudo sh setcaps.sh /path/to/MPF"
    exit 1
fi

for program in "$1/Programs/Redumper/bin/redumper" "$1/Programs/Creator/DiscImageCreator.out"; do
    if [ -f "$program" ]; then
        setcap cap_sys_rawio=ep "$program" && getcap "$program"
    else
        echo "Not found, skipped: $program"
    fi
done
